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

        public static System.Linq.Expressions.LambdaExpression EvaluateSelection(this Filter filter, IQueryable<Representation> representations, IQueryable<RepresentationAttribute> representationAttributes)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (filter.Expression == null)
            {
                throw new ArgumentNullException(nameof(filter.Expression));
            }

            if (representations == null)
            {
                throw new ArgumentNullException(nameof(representations));
            }

            if (representationAttributes == null)
            {
                throw new ArgumentNullException(nameof(representationAttributes));
            }


            var attrExpression = filter.Expression as AttributeExpression;
            if (attrExpression == null)
            {
                throw new ArgumentException("The filter is not a selector");
            }
            
            var representationParameter = LinqExpression.Parameter(typeof(Representation), "rp");
            var representationAttributeParameter = LinqExpression.Parameter(typeof(RepresentationAttribute), "ra");

            var enumerableType = typeof(Queryable);
            var enumerableMethods = enumerableType.GetMethods();
            var outerArg = LinqExpression.Parameter(typeof(Representation), "x");
            var innerArg = LinqExpression.Parameter(typeof(RepresentationAttribute), "y");
            var outerProperty = LinqExpression.Property(outerArg, "Id");
            var innerProperty = LinqExpression.Property(innerArg, "RepresentationId");
            var outerLambda = LinqExpression.Lambda(outerProperty, new System.Linq.Expressions.ParameterExpression[] { outerArg });
            var innerLambda = LinqExpression.Lambda(innerProperty, new System.Linq.Expressions.ParameterExpression[] { innerArg });
            var resultSelector = LinqExpression.Lambda(innerArg, new System.Linq.Expressions.ParameterExpression[] { outerArg, innerArg });
            var joinMethod = enumerableMethods.Where(m => m.Name == "Join" && m.IsGenericMethodDefinition).Where(m => m.GetParameters().ToList().Count == 5).First();
            var joinGenericMethod = joinMethod.MakeGenericMethod(typeof(Representation), typeof(RepresentationAttribute), typeof(string), typeof(RepresentationAttribute));
            var join = LinqExpression.Call(joinGenericMethod, LinqExpression.Constant(representations), LinqExpression.Constant(representationAttributes), outerLambda, innerLambda, resultSelector); // add join.

            var result = attrExpression.Path.EvaluateSelection(join);
            var finalSelectArg = LinqExpression.Parameter(typeof(IQueryable<Representation>), "f");
            var finalSelectRequestBody = LinqExpression.Lambda(result, new System.Linq.Expressions.ParameterExpression[] { finalSelectArg });
            return finalSelectRequestBody;
        }

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

        private static LinqExpression EvaluateSelection(this AttributePath path, LinqExpression outerExpression)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (outerExpression == null)
            {
                throw new ArgumentNullException(nameof(outerExpression));
            }

            var resourceAttrParameterExpr = LinqExpression.Parameter(typeof(RepresentationAttribute), "a");
            var equality = GetPathExpression(path, resourceAttrParameterExpr);
            var call = LinqExpression.Lambda<Func<RepresentationAttribute, bool>>(equality, resourceAttrParameterExpr); 
            var whereMethod = GetWhereMethod<RepresentationAttribute>();
            var result = LinqExpression.Call(whereMethod, outerExpression, call);
            var enumerableType = typeof(Queryable);
            var enumerableMethods = enumerableType.GetMethods();
            if (path.Next != null)
            {
                var next = path.Next;
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
                return EvaluateSelection(next, result);
            }

            return result;
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
            var equalExpr = selection;
            if (equalityExpr != null)
            {
                equalExpr = LinqExpression.AndAlso(selection, equalityExpr);
            }

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
                var anyExpr = BuildChildrenExpr(attributePath.ValueFilter.Expression, resourceAttrParameterExpr);
                result = LinqExpression.AndAlso(result, anyExpr);
            }

            if (attributePath.Next != null)
            {
                var anyExpr = EvaluateChildren(attributePath, resourceAttrParameterExpr, null);
                result = LinqExpression.AndAlso(result, anyExpr);
            }

            return result;
        }

        private static LinqExpression BuildChildrenExpr(Expression expr, System.Linq.Expressions.ParameterExpression resourceAttrParameterExpr)
        {
            var compExpr = expr as CompAttributeExpression;
            var logExpr = expr as LogicalExpression;
            if (compExpr != null)
            {
                var act = new Func<System.Linq.Expressions.ParameterExpression, LinqExpression>((p) =>
                {
                    return GetComparisonExpression(compExpr, p);
                });
                return EvaluateChildren(compExpr.Path, resourceAttrParameterExpr, act);
            }
            
            var leftExpr = logExpr.AttributeLeft;
            var rightExpr = logExpr.AttributeRight;
            var leftChildConds = BuildChildrenExpr(leftExpr, resourceAttrParameterExpr);
            var rightChildConds = BuildChildrenExpr(rightExpr, resourceAttrParameterExpr);
            LinqExpression result = null;
            switch(logExpr.Operator)
            {
                case LogicalOperators.and:
                    result = LinqExpression.AndAlso(leftChildConds, rightChildConds);
                    break;
                case LogicalOperators.or:
                    result = LinqExpression.OrElse(leftChildConds, rightChildConds);
                    break;
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

            var subArg = LinqExpression.Parameter(typeof(RepresentationAttribute), GetSc());
            var subChild = LinqExpression.Parameter(typeof(RepresentationAttribute), GetSc());
            var childrenProperty = LinqExpression.Property(arg, "Children");
            var argChildrenProperty = LinqExpression.Property(subArg, "Children");
            var schemaAttributeProperty = LinqExpression.Property(arg, "SchemaAttribute");
            var schemaTypeProperty = LinqExpression.Property(schemaAttributeProperty, "Type");
            var schemaMultiValuedProperty = LinqExpression.Property(schemaAttributeProperty, "MultiValued");

            var isMultiValued = LinqExpression.Equal(schemaMultiValuedProperty, LinqExpression.Constant(true)); // true
            var isNotMultiValued = LinqExpression.Equal(schemaMultiValuedProperty, LinqExpression.Constant(false));
            var isTypeComplex = LinqExpression.Equal(schemaTypeProperty, LinqExpression.Constant(Common.Constants.SchemaAttributeTypes.Complex));
            var isNotTypeComplex = LinqExpression.NotEqual(schemaTypeProperty, LinqExpression.Constant(Common.Constants.SchemaAttributeTypes.Complex));
            var isComplexMultiValued = LinqExpression.AndAlso(isMultiValued, isTypeComplex);
            var isNotComplexMultiValued = LinqExpression.OrElse(isNotTypeComplex, isNotMultiValued);

            var itemCount = LinqExpression.Call(typeof(Enumerable), "Count", new[] { typeof(RepresentationAttribute) }, childrenProperty);
            var moreThanZero = LinqExpression.GreaterThan(itemCount, LinqExpression.Constant(0));
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
             // a => aaze = azeaze
            var callEqualValue = LinqExpression.Lambda<Func<RepresentationAttribute, bool>>(result, subChild); // c => c.value = <value>
            var anyComplexMultiValuedSubAnyExpr = LinqExpression.Call(typeof(Enumerable), _anyMethodName, new[] { typeof(RepresentationAttribute) }, argChildrenProperty, callEqualValue);
            var lambdaMultiValuedSubAnyExpr = LinqExpression.Lambda<Func<RepresentationAttribute, bool>>(anyComplexMultiValuedSubAnyExpr, subArg);
            var anyComplexMultiValuedAnyExpr = LinqExpression.Call(typeof(Enumerable), _anyMethodName, new[] { typeof(RepresentationAttribute) }, childrenProperty, lambdaMultiValuedSubAnyExpr);
            var anyNotComplexMultiValuedExpr = LinqExpression.Call(typeof(Enumerable), _anyMethodName, new[] { typeof(RepresentationAttribute) }, childrenProperty, callEqualValue);
            var firstNotComplexMultiValued = LinqExpression.AndAlso(isNotComplexMultiValued, moreThanZero);
            var notComplexMultiValued = LinqExpression.AndAlso(firstNotComplexMultiValued, anyNotComplexMultiValuedExpr);
            var firstComplexMultiValued = LinqExpression.AndAlso(isComplexMultiValued, moreThanZero);
            var complexMultiValued = LinqExpression.AndAlso(firstComplexMultiValued, anyComplexMultiValuedAnyExpr);
            return LinqExpression.OrElse(complexMultiValued, notComplexMultiValued);
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
            var itemCount = LinqExpression.Call(typeof(Enumerable), "Count", new[] { typeof(RepresentationAttribute) }, representationAttributesProperty);
            var moreThanZero = LinqExpression.GreaterThan(itemCount, LinqExpression.Constant(0));
            return LinqExpression.AndAlso(moreThanZero, anyExpr);
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

            var propertySchemaAttribute = LinqExpression.Property(representationAttrExpr, "SchemaAttribute");
            var propertySchemaType = LinqExpression.Property(propertySchemaAttribute, "Type");
            var propertyValue = LinqExpression.Property(representationAttrExpr, "Value");
            var propertyValueNumber = LinqExpression.Property(representationAttrExpr, "ValueNumber");
            LinqExpression equalValue = null;
            switch (compAttributeExpression.Operator)
            {
                case ComparisonOperators.eq:
                    equalValue = LinqExpression.Equal(propertyValue, LinqExpression.Constant(compAttributeExpression.Value));
                    break;
                case ComparisonOperators.pr:
                    equalValue = LinqExpression.NotEqual(propertyValue, LinqExpression.Constant(null));
                    break;
                case ComparisonOperators.co:
                    equalValue = BuildComparisonExpression(compAttributeExpression, representationAttrExpr);
                    break;
                case ComparisonOperators.gt:
                    equalValue = LinqExpression.GreaterThan(propertyValueNumber, LinqExpression.Constant(double.Parse(compAttributeExpression.Value)));
                    break;
                case ComparisonOperators.ge:
                    equalValue = LinqExpression.GreaterThanOrEqual(propertyValueNumber, LinqExpression.Constant(double.Parse(compAttributeExpression.Value)));
                    break;
                case ComparisonOperators.le:
                    equalValue = LinqExpression.LessThanOrEqual(propertyValueNumber, LinqExpression.Constant(double.Parse(compAttributeExpression.Value)));
                    break;
                case ComparisonOperators.lt:
                    equalValue = LinqExpression.LessThan(propertyValueNumber, LinqExpression.Constant(double.Parse(compAttributeExpression.Value)));
                    break;
                case ComparisonOperators.sw:
                    var startWith = typeof(string).GetMethod("StartsWith");
                    equalValue = LinqExpression.Call(propertyValue, startWith, LinqExpression.Constant(compAttributeExpression.Value));
                    break;
                case ComparisonOperators.ew:
                    var endsWith = typeof(string).GetMethod("EndsWith");
                    equalValue = LinqExpression.Call(propertyValue, endsWith, LinqExpression.Constant(compAttributeExpression.Value));
                    break;
            }


            return equalValue;
        }

        private static LinqExpression BuildComparisonExpression(CompAttributeExpression compAttributeExpression, LinqExpression representationAttrExpr)
        {
            var argSchemaAttrValue = LinqExpression.Parameter(typeof(RepresentationAttributeValue), "rav");
            var propertyValue = LinqExpression.Property(representationAttrExpr, "Value");
            var propertySchemaAttribute = LinqExpression.Property(representationAttrExpr, "SchemaAttribute");
            var propertySchemaType = LinqExpression.Property(propertySchemaAttribute, "Type");
            var propertyMultiValued = LinqExpression.Property(propertySchemaAttribute, "MultiValued");
            var propertyValues = LinqExpression.Property(representationAttrExpr, "Values");
            var propertySchemaAttrValue = LinqExpression.Property(argSchemaAttrValue, "Value");

            var equality = LinqExpression.Equal(propertySchemaAttrValue, LinqExpression.Constant(compAttributeExpression.Value));
            var callEqualValue = LinqExpression.Lambda<Func<RepresentationAttributeValue, bool>>(equality, argSchemaAttrValue);
            var anyExpr = LinqExpression.Call(typeof(Enumerable), _anyMethodName, new[] { typeof(RepresentationAttributeValue) }, propertyValues, callEqualValue);
            var itemCount = LinqExpression.Call(typeof(Enumerable), "Count", new[] { typeof(RepresentationAttributeValue) }, propertyValues);
            var moreThanZero = LinqExpression.GreaterThan(itemCount, LinqExpression.Constant(0));
            var isMultiValued = LinqExpression.Equal(propertyMultiValued, LinqExpression.Constant(true));
            var isNotMultiValued = LinqExpression.Equal(propertyMultiValued, LinqExpression.Constant(false));

            var equalBoolean = LinqExpression.Equal(propertySchemaType, LinqExpression.Constant(Common.Constants.SchemaAttributeTypes.Boolean));
            var equalBinary = LinqExpression.Equal(propertySchemaType, LinqExpression.Constant(Common.Constants.SchemaAttributeTypes.Binary));
            var equalString = LinqExpression.Equal(propertySchemaType, LinqExpression.Constant(Common.Constants.SchemaAttributeTypes.String));
            var equalDatetime = LinqExpression.Equal(propertySchemaType, LinqExpression.Constant(Common.Constants.SchemaAttributeTypes.DateTime));
            var equalDecimal = LinqExpression.Equal(propertySchemaType, LinqExpression.Constant(Common.Constants.SchemaAttributeTypes.Decimal));
            var equalInteger = LinqExpression.Equal(propertySchemaType, LinqExpression.Constant(Common.Constants.SchemaAttributeTypes.Integer));
            var firstEqualityStr = LinqExpression.OrElse(equalBoolean, equalBinary);
            var secondEqualityStr = LinqExpression.OrElse(firstEqualityStr, equalString);
            var firstEqualityFloat = LinqExpression.OrElse(equalDatetime, equalDecimal);
            var equalityFloat = LinqExpression.OrElse(firstEqualityFloat, equalInteger);
            var finalEqualityStr = LinqExpression.OrElse(equalityFloat, secondEqualityStr);
            var containsStr = LinqExpression.AndAlso(moreThanZero, anyExpr);
            var equalityMultipleStr = LinqExpression.AndAlso(isMultiValued, containsStr);
            
            var contains = typeof(string).GetMethod("Contains");
            var containsEqualValue = LinqExpression.Call(propertyValue, contains, LinqExpression.Constant(compAttributeExpression.Value));
            var equalityStr = LinqExpression.AndAlso(isNotMultiValued, containsEqualValue);

            var checkStrMultiValued = LinqExpression.AndAlso(finalEqualityStr, equalityMultipleStr);
            var checkContainsStr = LinqExpression.AndAlso(finalEqualityStr, equalityStr);
            return LinqExpression.OrElse(checkStrMultiValued, checkContainsStr);
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
