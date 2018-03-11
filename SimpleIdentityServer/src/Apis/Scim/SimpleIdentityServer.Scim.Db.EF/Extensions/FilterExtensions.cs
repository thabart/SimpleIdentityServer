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
        public static LinqExpression Evaluate(this Filter filter, IEnumerable<RepresentationAttribute> representationAttributes)
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

        public static LinqExpression Evaluate(this Expression expression, IEnumerable<RepresentationAttribute> representationAttributes)
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

        public static LinqExpression Evaluate(this AttributeExpression expression, IEnumerable<RepresentationAttribute> representationAttributes)
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
            return path.Evaluate(LinqExpression.Constant(representationAttributes), true);
        }

        public static LinqExpression Evaluate(this AttributePath attributePath, LinqExpression outerExpression, bool selectResult = false)
        {
            if (attributePath == null)
            {
                throw new ArgumentNullException(nameof(attributePath));
            }
            
            var resourceAttrParameterExpr = LinqExpression.Parameter(typeof(RepresentationAttribute), "a");
            var schemaAttributeParameter = LinqExpression.Property(resourceAttrParameterExpr, "SchemaAttribute");
            var propertyName = LinqExpression.Property(schemaAttributeParameter, "Name");
            var equalName = LinqExpression.Equal(propertyName, LinqExpression.Constant(attributePath.Name)); // name = <path>
            var call = LinqExpression.Lambda<Func<RepresentationAttribute, bool>>(equalName, resourceAttrParameterExpr); // a => a.name = <path>
            var whereMethod = GetWhereMethod<RepresentationAttribute>();
            var result = LinqExpression.Call(whereMethod, outerExpression, call); // where(a => a.name = <path>)
            var enumerableType = typeof(Queryable);
            var enumerableMethods = enumerableType.GetMethods();
            if (attributePath.ValueFilter != null) // where(a => a.name = <path>).where(a => a.Children.any(<instruction>))
            {
                // where (a => a.any)
                EvaluateValueFilter(attributePath.ValueFilter.Expression as CompAttributeExpression);
                // attributePath.ValueFilter.Evaluate();
            }

            if (attributePath.Next != null)
            {
                var next = attributePath.Next;
                var outerArg = LinqExpression.Parameter(typeof(RepresentationAttribute), "x");
                var innerArg = LinqExpression.Parameter(typeof(RepresentationAttribute), "y");
                var outerProperty = LinqExpression.Property(outerArg, "Id");
                var innerProperty = LinqExpression.Property(innerArg, "RepresentationAttributeIdParent");
                var outerLambda = LinqExpression.Lambda(outerProperty, new System.Linq.Expressions.ParameterExpression[] { outerArg });
                var innerLambda = LinqExpression.Lambda(innerProperty, new System.Linq.Expressions.ParameterExpression[] { innerArg });
                var resultSelector = LinqExpression.Lambda(innerArg, new System.Linq.Expressions.ParameterExpression[] { outerArg, innerArg });
                var joinMethod = enumerableMethods.Where(m => m.Name == "Join" && m.IsGenericMethodDefinition).Where(m => m.GetParameters().ToList().Count == 5).First();
                var joinGenericMethod = joinMethod.MakeGenericMethod(typeof(RepresentationAttribute), typeof(RepresentationAttribute), typeof(string), typeof(RepresentationAttribute));
                result = LinqExpression.Call(joinGenericMethod, result, outerExpression, outerLambda, innerLambda, resultSelector); // add join.
                return Evaluate(next, result, selectResult);
            }
            else if (selectResult)
            {
                var selectArg = LinqExpression.Parameter(typeof(RepresentationAttribute), "r");
                var selectProperty = LinqExpression.Property(selectArg, "Value");
                var selectLambda = LinqExpression.Lambda(selectProperty, new System.Linq.Expressions.ParameterExpression[] { selectArg });
                var selectMethod = enumerableMethods.Where(m => m.Name == "Select" && m.IsGenericMethodDefinition).Where(m => m.GetParameters().ToList().Count() == 2).First();
                var selectGenericMethod = selectMethod.MakeGenericMethod(typeof(RepresentationAttribute), typeof(string));
                return LinqExpression.Call(selectGenericMethod, result, selectLambda);
            }

            return result;
        }

        /// <summary>
        /// Evaluates the filter and returns where expression.
        /// </summary>
        /// <param name="compAttributeExpression"></param>
        /// <param name="representationAttributes"></param>
        /// <returns></returns>
        public static LinqExpression Evaluate(this CompAttributeExpression compAttributeExpression, LinqExpression outerExpress)
        {
            if (compAttributeExpression == null)
            {
                throw new ArgumentNullException(nameof(compAttributeExpression));
            }

            if (outerExpress == null)
            {
                throw new ArgumentNullException(nameof(outerExpress));
            }

            var selection = compAttributeExpression.Path.Evaluate(outerExpress);
            var resourceAttrParameterExpr = LinqExpression.Parameter(typeof(RepresentationAttribute), "r");
            var propertyValue = LinqExpression.Property(resourceAttrParameterExpr, "Value");
            LinqExpression equalValue = null;
            switch (compAttributeExpression.Operator)
            {
                case ComparisonOperators.eq:
                    equalValue = LinqExpression.Equal(propertyValue, LinqExpression.Constant(compAttributeExpression.Value)); // value = <value>
                    break;
            }

            var call = LinqExpression.Lambda<Func<RepresentationAttribute, bool>>(equalValue, resourceAttrParameterExpr);
            var whereMethod = GetWhereMethod<RepresentationAttribute>();
            var callWhereMethod = LinqExpression.Call(whereMethod, selection, call);
            return callWhereMethod;
        }

        private static LinqExpression EvaluateValueFilter(CompAttributeExpression compAttrExpr)
        {
            var anyArg = LinqExpression.Parameter(typeof(RepresentationAttribute), "rp");
            var childrenProperty = LinqExpression.Property(anyArg, "Children");
            var resourceAttrParameterExpr = LinqExpression.Parameter(typeof(RepresentationAttribute), "a");
            var schemaAttributeParameter = LinqExpression.Property(resourceAttrParameterExpr, "SchemaAttribute");
            var propertyName = LinqExpression.Property(schemaAttributeParameter, "Name");
            var equalName = LinqExpression.Equal(propertyName, LinqExpression.Constant(compAttrExpr.Path.Name)); // name = <path>
            var callEqualValue = LinqExpression.Lambda<Func<RepresentationAttribute, bool>>(equalName, resourceAttrParameterExpr); // c => c.value = <value>

            var tmpp = LinqExpression.Call(typeof(Enumerable), "Any", new[] { typeof(RepresentationAttribute) }, childrenProperty, callEqualValue);
            /*
            var anyMethod = enumerableMethods
             .Where(m => m.Name == "Any" && m.IsGenericMethodDefinition)
             .Where(m => m.GetParameters().Count() == 2).First().MakeGenericMethod(typeof(RepresentationAttribute));
            var anyGenericMethod = anyMethod.MakeGenericMethod(typeof(RepresentationAttribute), typeof(string));
            LinqExpression.Call(anyGenericMethod, selection, equalValue); // any(c => c.value = <value>)
            */
            return null;
        }

        private static MethodInfo GetWhereMethod<T>()
        {
            var enumarableType = typeof(Queryable);
            var genericMethod = enumarableType.GetMethods()
                 .Where(m => m.Name == "Where" && m.IsGenericMethodDefinition)
                 .Where(m => m.GetParameters().Count() == 2).First().MakeGenericMethod(typeof(T));
            return genericMethod;
        }

        private static MethodInfo GetAnyMethod<T>()
        {
            var enumarableType = typeof(Queryable);
            var genericMethod = enumarableType.GetMethods()
                 .Where(m => m.Name == "Any" && m.IsGenericMethodDefinition)
                 .Where(m => m.GetParameters().Count() == 2).First().MakeGenericMethod(typeof(T));
            return genericMethod;
        }
    }
}
