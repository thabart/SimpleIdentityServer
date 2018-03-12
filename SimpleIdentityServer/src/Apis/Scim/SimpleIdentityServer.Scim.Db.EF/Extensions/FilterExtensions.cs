using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Db.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LinqExpression = System.Linq.Expressions.Expression;

namespace SimpleIdentityServer.Scim.Db.EF.Extensions
{
    public static class FilterExtensions
    {
        private static int _scI = 0;
        private static string _whereMethodName = "Where";
        private static string _anyMethodName = "Any";

        public static System.Linq.Expressions.LambdaExpression Evaluate(this Filter filter, IEnumerable<RepresentationAttribute> representationAttributes)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (representationAttributes == null)
            {
                throw new ArgumentNullException(nameof(representationAttributes));
            }

            if (filter.Expression == null)
            {
                throw new ArgumentNullException(nameof(filter.Expression));
            }

            return filter.Expression.Evaluate(representationAttributes);
        }

        public static System.Linq.Expressions.LambdaExpression Evaluate(this Expression expression, IEnumerable<RepresentationAttribute> representationAttributes)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (representationAttributes == null)
            {
                throw new ArgumentNullException(nameof(representationAttributes));
            }

            var compAttrExpression = expression as CompAttributeExpression;
            var attrExpression = expression as AttributeExpression;
            if (compAttrExpression != null)
            {
                return compAttrExpression.Evaluate(representationAttributes);
            }

            if (attrExpression != null)
            {
                return attrExpression.Evaluate(representationAttributes);
            }

            return null;
        }

        public static System.Linq.Expressions.LambdaExpression Evaluate(this AttributeExpression expression, IEnumerable<RepresentationAttribute> representationAttributes)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(representationAttributes));
            }

            if (expression.Path == null)
            {
                throw new ArgumentNullException(nameof(expression.Path));
            }

            var path = expression.Path;

            var resourceAttrParameterExpr = LinqExpression.Parameter(typeof(RepresentationAttribute), "ra");
            var result = path.Evaluate(resourceAttrParameterExpr);
            var whereLambda = LinqExpression.Lambda<Func<RepresentationAttribute, bool>>(result, resourceAttrParameterExpr);
            var whereMethod = GetWhereMethod<RepresentationAttribute>();
            var whereExpr = LinqExpression.Call(whereMethod, LinqExpression.Constant(representationAttributes), whereLambda);
            var finalSelectArg = LinqExpression.Parameter(typeof(IQueryable<RepresentationAttribute>), "f");
            var finalSelectRequestBody = LinqExpression.Lambda(whereExpr, new System.Linq.Expressions.ParameterExpression[] { finalSelectArg });
            return finalSelectRequestBody;
        }

        public static System.Linq.Expressions.LambdaExpression Evaluate(this CompAttributeExpression expression, IEnumerable<RepresentationAttribute> representationAttributes)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(representationAttributes));
            }

            if (expression.Path == null)
            {
                throw new ArgumentNullException(nameof(expression.Path));
            }

            var resourceAttrParameterExpr = LinqExpression.Parameter(typeof(RepresentationAttribute), "ra");
            var expr = expression.Evaluate(resourceAttrParameterExpr);
            var whereLambda = LinqExpression.Lambda<Func<RepresentationAttribute, bool>>(expr, resourceAttrParameterExpr);
            var whereMethod = GetWhereMethod<RepresentationAttribute>();
            var callWhereMethod = LinqExpression.Call(whereMethod, LinqExpression.Constant(representationAttributes), whereLambda);
            var finalSelectArg = LinqExpression.Parameter(typeof(IQueryable<RepresentationAttribute>), "f");
            var finalSelectRequestBody = LinqExpression.Lambda(callWhereMethod, new System.Linq.Expressions.ParameterExpression[] { finalSelectArg });
            return finalSelectRequestBody;
        }

        public static LinqExpression Evaluate(this AttributePath attributePath, System.Linq.Expressions.ParameterExpression resourceAttrParameterExpr)
        {
            if (attributePath == null)
            {
                throw new ArgumentNullException(nameof(attributePath));
            }
            
            var result = GetPathExpression(attributePath, resourceAttrParameterExpr);

            if (attributePath.ValueFilter != null)
            {
                var compExpr = (CompAttributeExpression)attributePath.ValueFilter.Expression;
                var act = new Func<System.Linq.Expressions.ParameterExpression, LinqExpression>((p) =>
                {
                    return GetComparisonExpression(compExpr, p);
                });
                var anyExpr = EvaluateChildren(compExpr.Path, resourceAttrParameterExpr, act);
                result = LinqExpression.AndAlso(result, anyExpr);
            }

            if (attributePath.Next != null)
            {
                var anyExpr = EvaluateChildren(attributePath, resourceAttrParameterExpr, null);
                result = LinqExpression.AndAlso(result, anyExpr);
            }

            return result;
        }
        
        public static LinqExpression Evaluate(this CompAttributeExpression compAttributeExpression,  System.Linq.Expressions.ParameterExpression arg)
        {
            if (compAttributeExpression == null)
            {
                throw new ArgumentNullException(nameof(compAttributeExpression));
            }

            if (arg == null)
            {
                throw new ArgumentNullException(nameof(arg));
            }

            var selection = compAttributeExpression.Path.Evaluate(arg);
            var equalityExpr = GetComparisonExpression(compAttributeExpression, arg);
            return LinqExpression.AndAlso(selection, equalityExpr);
        }

        private static LinqExpression EvaluateChildren(AttributePath path, System.Linq.Expressions.ParameterExpression arg, Func<System.Linq.Expressions.ParameterExpression, LinqExpression> callback = null)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (arg == null)
            {
                throw new ArgumentNullException(nameof(arg));
            }

            var subChild = LinqExpression.Parameter(typeof(RepresentationAttribute), GetSc());
            var childrenProperty = LinqExpression.Property(arg, "Children");
            var equalNotNull = LinqExpression.NotEqual(childrenProperty, LinqExpression.Constant(null));
            LinqExpression result = null;
            if (callback != null)
            {
                result = callback(subChild);
            }

            if (path.Next != null)
            {
                var subCond = path.Next.Evaluate(subChild);
                if (result != null)
                {
                    result = LinqExpression.AndAlso(result, subCond);
                }
                else
                {
                    result = subCond;
                }
            }

            var callEqualValue = LinqExpression.Lambda<Func<RepresentationAttribute, bool>>(result, subChild); // c => c.value = <value>
            var anyExpr = LinqExpression.Call(typeof(Enumerable), _anyMethodName, new[] { typeof(RepresentationAttribute) }, childrenProperty, callEqualValue);
            return LinqExpression.AndAlso(equalNotNull, anyExpr);
        }

        #region Private static methods

        private static LinqExpression GetPathExpression(AttributePath path, LinqExpression representationAttrExpr)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (representationAttrExpr == null)
            {
                throw new ArgumentNullException(nameof(representationAttrExpr));
            }

            var schemaAttributeParameter = LinqExpression.Property(representationAttrExpr, "SchemaAttribute");
            var propertyName = LinqExpression.Property(schemaAttributeParameter, "Name");
            var notNull = LinqExpression.NotEqual(schemaAttributeParameter, LinqExpression.Constant(null));
            var equal = LinqExpression.Equal(propertyName, LinqExpression.Constant(path.Name));
            return LinqExpression.AndAlso(notNull, equal);
        }

        private static LinqExpression GetComparisonExpression(CompAttributeExpression compAttributeExpression)
        {
            if (compAttributeExpression == null)
            {
                throw new ArgumentNullException(nameof(compAttributeExpression));
            }

            var resourceAttrParameterExpr = LinqExpression.Parameter(typeof(RepresentationAttribute), "r");
            var equalValue = GetComparisonExpression(compAttributeExpression, resourceAttrParameterExpr);
            return LinqExpression.Lambda<Func<RepresentationAttribute, bool>>(equalValue, resourceAttrParameterExpr);
        }

        private static LinqExpression GetComparisonExpression(CompAttributeExpression compAttributeExpression, LinqExpression representationAttrExpr)
        {
            if (compAttributeExpression == null)
            {
                throw new ArgumentNullException(nameof(compAttributeExpression));
            }

            if (representationAttrExpr == null)
            {
                throw new ArgumentNullException(nameof(representationAttrExpr));
            }

            var propertyValue = LinqExpression.Property(representationAttrExpr, "Value");
            LinqExpression equalValue = null;
            switch (compAttributeExpression.Operator)
            {
                case ComparisonOperators.eq:
                    equalValue = LinqExpression.Equal(propertyValue, LinqExpression.Constant(compAttributeExpression.Value));
                    break;
            }


            return equalValue;
        }

        private static MethodInfo GetWhereMethod<T>()
        {
            var enumarableType = typeof(Queryable);
            var genericMethod = enumarableType.GetMethods()
                 .Where(m => m.Name == _whereMethodName && m.IsGenericMethodDefinition)
                 .Where(m => m.GetParameters().Count() == 2).First().MakeGenericMethod(typeof(T));
            return genericMethod;
        }

        private static string GetSc()
        {
            _scI++;
            return $"sc{_scI}";
        }

        #endregion
    }
}
