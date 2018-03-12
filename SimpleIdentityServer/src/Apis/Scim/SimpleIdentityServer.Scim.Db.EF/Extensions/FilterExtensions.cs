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

        public static System.Linq.Expressions.LambdaExpression EvaluateFilter(this Filter filter, IQueryable<Representation> representations)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (representations == null)
            {
                throw new ArgumentNullException(nameof(representations));
            }

            if (filter.Expression == null)
            {
                throw new ArgumentNullException(nameof(filter.Expression));
            }

            return filter.Expression.Evaluate(representations);
        }

        #region Private static methods

        private static System.Linq.Expressions.LambdaExpression Evaluate(this Expression expression, IQueryable<Representation> representations)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (representations == null)
            {
                throw new ArgumentNullException(nameof(representations));
            }

            var representationParameter = LinqExpression.Parameter(typeof(Representation), "rp");
            var resourceAttrParameterExpr = LinqExpression.Parameter(typeof(RepresentationAttribute), "ra");
            var representationAttributesProperty = LinqExpression.Property(representationParameter, "Attributes");
            var result = expression.Evaluate(representationParameter);
            if (result == null)
            {
                return null;
            }

            var whereMethod = GetWhereMethod<Representation>();
            var equalLambda = LinqExpression.Lambda<Func<Representation, bool>>(result, representationParameter);
            var whereExpr = LinqExpression.Call(whereMethod, LinqExpression.Constant(representations), equalLambda);
            var finalSelectArg = LinqExpression.Parameter(typeof(IQueryable<Representation>), "f");
            var finalSelectRequestBody = LinqExpression.Lambda(whereExpr, new System.Linq.Expressions.ParameterExpression[] { finalSelectArg });
            return finalSelectRequestBody;
        }

        private static LinqExpression Evaluate(this Expression expression, System.Linq.Expressions.ParameterExpression resourceAttrParameterExpr)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var compAttrExpression = expression as CompAttributeExpression;
            var attrExpression = expression as AttributeExpression;
            var logicalExpression = expression as LogicalExpression;
            if (compAttrExpression != null)
            {
                return compAttrExpression.Evaluate(resourceAttrParameterExpr);
            }

            if (attrExpression != null)
            {
                return attrExpression.Evaluate(resourceAttrParameterExpr);
            }

            if (logicalExpression != null)
            {
                return logicalExpression.Evaluate(resourceAttrParameterExpr);
            }

            return null;
        }

        private static LinqExpression Evaluate(this AttributeExpression expr, System.Linq.Expressions.ParameterExpression arg)
        {
            if (expr == null)
            {
                throw new ArgumentNullException(nameof(expr));
            }

            if (expr.Path == null)
            {
                throw new ArgumentNullException(nameof(expr.Path));
            }

            if (arg == null)
            {
                throw new ArgumentNullException(nameof(arg));
            }

            var resourceAttrParameterExpr = LinqExpression.Parameter(typeof(RepresentationAttribute), "ra");
            var selection = expr.Path.Evaluate(resourceAttrParameterExpr);
            var andEqual = GetAttributesAny(selection, arg, resourceAttrParameterExpr);
            return andEqual;
        }

        private static LinqExpression EvaluateSelection(this AttributePath path, LinqExpression arg)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (arg == null)
            {
                throw new ArgumentNullException(nameof(arg));
            }

            var property = LinqExpression.Property(arg, path.Name);
            if (path.Next != null)
            {
                return path.Next.EvaluateSelection(property);
            }

            return property;
        }

        private static LinqExpression Evaluate(this CompAttributeExpression compAttributeExpression,  System.Linq.Expressions.ParameterExpression arg)
        {
            if (compAttributeExpression == null)
            {
                throw new ArgumentNullException(nameof(compAttributeExpression));
            }

            if (arg == null)
            {
                throw new ArgumentNullException(nameof(arg));
            }

            var resourceAttrParameterExpr = LinqExpression.Parameter(typeof(RepresentationAttribute), "ra");
            var representationAttributesProperty = LinqExpression.Property(arg, "Attributes");

            var selection = compAttributeExpression.Path.Evaluate(resourceAttrParameterExpr);
            var equalityExpr = GetComparisonExpression(compAttributeExpression, resourceAttrParameterExpr);
            var equalExpr = LinqExpression.AndAlso(selection, equalityExpr);
            var andEqual = GetAttributesAny(equalExpr, arg, resourceAttrParameterExpr);
            return andEqual;
        }

        private static LinqExpression Evaluate(this LogicalExpression expression, System.Linq.Expressions.ParameterExpression arg)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (arg == null)
            {
                throw new ArgumentNullException(nameof(arg));
            }

            LinqExpression leftExpression = null;
            LinqExpression rightExpression = null;
            if (expression.AttributeLeft != null)
            {
                leftExpression = expression.AttributeLeft.Evaluate(arg);
            }

            if (expression.AttributeRight != null)
            {
                rightExpression = expression.AttributeRight.Evaluate(arg);
            }

            if (leftExpression != null && rightExpression != null)
            {
                switch(expression.Operator)
                {
                    case LogicalOperators.and:
                        return LinqExpression.And(leftExpression, rightExpression);
                    case LogicalOperators.or:
                        return LinqExpression.Or(leftExpression, rightExpression);
                }
            }

            if (leftExpression != null)
            {
                return leftExpression;
            }

            if (rightExpression != null)
            {
                return rightExpression;
            }

            return null;
        }

        private static LinqExpression Evaluate(this AttributePath attributePath, System.Linq.Expressions.ParameterExpression resourceAttrParameterExpr)
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

        private static LinqExpression GetAttributesAny(LinqExpression expr, System.Linq.Expressions.ParameterExpression arg, System.Linq.Expressions.ParameterExpression raArg)
        {
            if (expr == null)
            {
                throw new ArgumentNullException(nameof(expr));
            }

            if (arg == null)
            {
                throw new ArgumentNullException(nameof(arg));
            }
            
            var representationAttributesProperty = LinqExpression.Property(arg, "Attributes");
            var anyLambda = LinqExpression.Lambda<Func<RepresentationAttribute, bool>>(expr, raArg);
            var anyExpr = LinqExpression.Call(typeof(Enumerable), _anyMethodName, new[] { typeof(RepresentationAttribute) }, representationAttributesProperty, anyLambda);
            var attributesNotEqual = LinqExpression.NotEqual(representationAttributesProperty, LinqExpression.Constant(null));
            return LinqExpression.AndAlso(attributesNotEqual, anyExpr);
        }

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
