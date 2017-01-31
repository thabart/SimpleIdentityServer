#region copyright
// Copyright 2017 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace SimpleIdentityServer.EventStore.EF.Extensions
{
    internal static class EnumerableExtensions
    {
        private static Dictionary<string, string> _mappingJsonNameToModelName = new Dictionary<string, string>
        {
            { Core.Common.EventResponseNames.Id, "Id" },
            { Core.Common.EventResponseNames.AggregateId, "AggregateId" },
            { Core.Common.EventResponseNames.Payload, "Payload" },
            { Core.Common.EventResponseNames.Description, "Description" },
            { Core.Common.EventResponseNames.CreatedOn, "CreatedOn" }
        };

        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IEnumerable<TSource> query, string propertyName)
        {
            var rule = _mappingJsonNameToModelName.FirstOrDefault(m => m.Key == propertyName);
            if (rule.Equals(default(KeyValuePair<string, string>)) || string.IsNullOrWhiteSpace(rule.Value))
            {
                throw new InvalidOperationException("the property doesn't exist");
            }

            var entityType = typeof(TSource);
            var propertyInfo = entityType.GetProperty(rule.Value);
            ParameterExpression arg = Expression.Parameter(entityType, "x");
            MemberExpression property = Expression.Property(arg, rule.Value);
            var selector = Expression.Lambda(property, new ParameterExpression[] { arg });
            var enumarableType = typeof(Queryable);
            var method = enumarableType.GetMethods()
                 .Where(m => m.Name == "OrderBy" && m.IsGenericMethodDefinition)
                 .Where(m =>
                 {
                     var parameters = m.GetParameters().ToList();       
             return parameters.Count == 2;
         }).Single();
            MethodInfo genericMethod = method
                 .MakeGenericMethod(entityType, propertyInfo.PropertyType);
            var newQuery = (IOrderedQueryable<TSource>)genericMethod
                 .Invoke(genericMethod, new object[] { query, selector });
            return newQuery;
        }

        public static IOrderedQueryable<TSource> OrderByDescending<TSource>(this IEnumerable<TSource> query, string propertyName)
        {
            var rule = _mappingJsonNameToModelName.FirstOrDefault(m => m.Key == propertyName);
            if (rule.Equals(default(KeyValuePair<string, string>)) || string.IsNullOrWhiteSpace(rule.Value))
            {
                throw new InvalidOperationException("the property doesn't exist");
            }

            var entityType = typeof(TSource);
            var propertyInfo = entityType.GetProperty(rule.Value);
            ParameterExpression arg = Expression.Parameter(entityType, "x");
            MemberExpression property = Expression.Property(arg, rule.Value);
            var selector = Expression.Lambda(property, new ParameterExpression[] { arg });
            var enumarableType = typeof(System.Linq.Queryable);
            var method = enumarableType.GetMethods()
                 .Where(m => m.Name == "OrderByDescending" && m.IsGenericMethodDefinition)
                 .Where(m =>
                 {
                     var parameters = m.GetParameters().ToList();
                     return parameters.Count == 2;
                 }).Single();
            MethodInfo genericMethod = method
                 .MakeGenericMethod(entityType, propertyInfo.PropertyType);
            var newQuery = (IOrderedQueryable<TSource>)genericMethod
                 .Invoke(genericMethod, new object[] { query, selector });
            return newQuery;
        }

        public static IQueryable<TSource> Where<TSource>(this IEnumerable<TSource> query, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var splitted = Regex.Split(Regex.Replace(filter, @"\s+", ""), "eq");
            if (splitted.Count() != 2)
            {
                throw new ArgumentException("the filter is not correct");
            }

            var propertyName = splitted.First();
            var value = splitted.ElementAt(1);            
            var entityType = typeof(TSource);
            var propertyInfo = entityType.GetProperty(propertyName);
            var arg = Expression.Parameter(entityType, "x");
            var property = Expression.Property(arg, propertyName);
            var equalExpr = Expression.Equal(property, Expression.Constant(value));
            var selector = Expression.Lambda(equalExpr, new ParameterExpression[] { arg });
            var enumarableType = typeof(Queryable);
            var methods = enumarableType.GetMethods()
                 .Where(m => m.Name == "Where" && m.IsGenericMethodDefinition)
                 .Where(m =>
                 {
                     var parameters = m.GetParameters().ToList();
                     return parameters.Count == 2;
                 });
            MethodInfo genericMethod = methods.First()
                 .MakeGenericMethod(entityType);
            var newQuery = (IOrderedQueryable<TSource>)genericMethod
                 .Invoke(genericMethod, new object[] { query, selector });
            return newQuery;
        }

        public static IQueryable<IGrouping<object, TSource>> GroupBy<TSource>(this IQueryable<TSource> query, string groupBy)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (string.IsNullOrWhiteSpace(groupBy))
            {
                throw new ArgumentNullException(nameof(groupBy));
            }

            var entityType = typeof(TSource);
            ParameterExpression arg = Expression.Parameter(entityType, "x");
            var fieldNames = groupBy.Split(',');
            LambdaExpression selector = null;
            var enumarableType = typeof(Queryable);
            var method = enumarableType.GetMethods()
                 .Where(m => m.Name == "GroupBy" && m.IsGenericMethodDefinition)
                 .Where(m =>
                 {
                     var parameters = m.GetParameters().ToList();
                     return parameters.Count == 2;
                 }).Single();
            MethodInfo genericMethod;
            if (fieldNames.Count() == 1)
            {
                var propertyInfo = entityType.GetProperty(groupBy);
                MemberExpression keyProperty = Expression.Property(arg, groupBy);
                var keySelector = Expression.Lambda(keyProperty, new ParameterExpression[] { arg });
                genericMethod = method.MakeGenericMethod(entityType, propertyInfo.PropertyType);
                selector = Expression.Lambda(keyProperty, new ParameterExpression[] { arg });
            }
            else
            {
                var sourceProperties = fieldNames.ToDictionary(name => name, name => query.ElementType.GetProperty(name));
                var anonType = CreateNewAnonymousType<TSource>(fieldNames);
                var bindings = anonType.GetFields().Select(p => Expression.Bind(p, Expression.Property(arg, sourceProperties[p.Name]))).OfType<MemberBinding>();
                selector = Expression.Lambda(Expression.MemberInit(Expression.New(anonType.GetConstructor(Type.EmptyTypes)), bindings), arg);
                genericMethod = method.MakeGenericMethod(entityType, anonType.AsType());
            }

            var newQuery = (IQueryable<IGrouping<object, TSource>>)genericMethod
                 .Invoke(genericMethod, new object[] { query, selector });
            return newQuery;
        }

        public static IQueryable<dynamic> Select<TSource>(this IQueryable<TSource> query, string filter)
        {
            var entityType = typeof(TSource);
            string[] fieldNames = null;
            if (!string.IsNullOrWhiteSpace(filter))
            {
                fieldNames = filter.Split(',');
            }
            else
            {
                fieldNames = entityType.GetProperties().Select(m => m.Name).ToArray();
            }

            var arg = Expression.Parameter(entityType, "x");
            LambdaExpression selector = null;
            Type type = null;
            if (fieldNames.Count() == 1)
            {
                var field = fieldNames.First();
                var propertyInfo = entityType.GetProperty(field);
                MemberExpression property = Expression.Property(arg, field);
                selector = Expression.Lambda(property, new ParameterExpression[] { arg });
                type = propertyInfo.PropertyType;
            }
            else
            {
                var sourceProperties = fieldNames.ToDictionary(name => name, name => query.ElementType.GetProperty(name));
                var anonType = CreateNewAnonymousType<TSource>(fieldNames);
                var bindings = anonType.GetFields().Select(p => Expression.Bind(p, Expression.Property(arg, sourceProperties[p.Name]))).OfType<MemberBinding>();
                selector = Expression.Lambda(Expression.MemberInit(Expression.New(anonType.GetConstructor(Type.EmptyTypes)), bindings), arg);
                type = anonType.AsType();
            }

            return (IQueryable<dynamic>)query.Provider.CreateQuery(
                Expression.Call(typeof(Queryable), "Select", new Type[] { typeof(TSource), type },
                 Expression.Constant(query), selector));
        }

        public static IQueryable<TSource> Join<TSource>(this IQueryable<TSource> query, string filter)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (string.IsNullOrWhiteSpace(filter))
            {
                throw new ArgumentNullException(nameof(filter));
            }
            
            var tuple = filter.Split('|');
            if (tuple.Count() != 2)
            {
                throw new ArgumentException("the filter is not correct");
            }

            var outerKey = tuple.First();
            var innerKey = tuple.ElementAt(1);
            var entityType = typeof(TSource);
            var propertyInfoOuter = entityType.GetProperty(outerKey);
            var propertyInfoInner = entityType.GetProperty(innerKey);
            ParameterExpression arg = Expression.Parameter(entityType, "x");
            MemberExpression propertyOuter = Expression.Property(arg, outerKey);
            MemberExpression propertyInner = Expression.Property(arg, innerKey);
            var selectorPropertyOuter = Expression.Lambda(propertyOuter, new ParameterExpression[] { arg });
            var selectorPropertyInner = Expression.Lambda(propertyInner, new ParameterExpression[] { arg });
            var enumarableType = typeof(Queryable);
            var method = enumarableType.GetMethods()
                 .Where(m => m.Name == "OrderBy" && m.IsGenericMethodDefinition)
                 .Where(m =>
                 {
                     return m.GetParameters().ToList().Count == 5;
                 }).First();
            MethodInfo genericMethod = method.MakeGenericMethod(entityType, propertyInfoOuter.PropertyType, propertyInfoInner.PropertyType, entityType);
            var newQuery = (IQueryable<TSource>)genericMethod
                 .Invoke(genericMethod, new object[] { query, propertyOuter, propertyInner,  null});
            return newQuery;
        }

        private static TypeInfo CreateNewAnonymousType<TSource>(IEnumerable<string> fieldNames)
        {
            var dynamicAssemblyName = new AssemblyName("TempAssm");
            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
            var dynamicModule = dynamicAssembly.DefineDynamicModule("TempAssm");
            TypeBuilder dynamicAnonymousType = dynamicModule.DefineType("TempCl", TypeAttributes.Public);
            var modelType = typeof(TSource);
            foreach(var fieldName in fieldNames)
            {
                var property = modelType.GetProperty(fieldName);
                dynamicAnonymousType.DefineField(fieldName, property.PropertyType, FieldAttributes.Public);
            }

            return dynamicAnonymousType.CreateTypeInfo();
        }
    }
}
