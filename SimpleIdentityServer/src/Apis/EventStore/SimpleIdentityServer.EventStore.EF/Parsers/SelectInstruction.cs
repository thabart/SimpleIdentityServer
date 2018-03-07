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
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using SimpleIdentityServer.EventStore.EF.Extensions;

namespace SimpleIdentityServer.EventStore.EF.Parsers
{
    public class SelectInstruction : BaseInstruction
    {
        public const string Name = "select";
        private const string _starSelector = "*";

        public override KeyValuePair<string, Expression>? GetExpression<TSource>(Type sourceType, ParameterExpression rootParameter, IEnumerable<TSource> source)
        {
            IEnumerable<string> fieldNames = null;
            if (!string.IsNullOrWhiteSpace(Parameter) && Parameter != _starSelector)
            {
                fieldNames = Parameter.GetParameters();
            }
            else
            {
                fieldNames = sourceType.GetProperties().Select(m => m.Name).ToArray();
            }

            // 1. Get sub expression.
            Expression subExpr = null;
            Type targetType = sourceType;
            if (SubInstruction != null)
            {
                var kvp = SubInstruction.GetExpression(sourceType, rootParameter, source).Value;
                subExpr = kvp.Value;
                targetType = subExpr.Type.GetGenericArguments().First();
                if (string.IsNullOrWhiteSpace(Parameter))
                {
                    return kvp;
                }
            }

            // x
            var arg = Expression.Parameter(targetType, "x");
            // f
            var finalSelectArg = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(targetType), "f");
            LambdaExpression selector = null;
            Type type = null;
            if (fieldNames.Count() == 1)
            {
                var field = fieldNames.First();
                MemberExpression property = Expression.Property(arg, field);
                selector = Expression.Lambda(property, new ParameterExpression[] { arg });
                type = property.Type;
            }
            else
            {
                var sourceProperties = fieldNames.ToDictionary(name => name, name => sourceType.GetProperty(name));
                var anonType = ReflectionHelper.CreateNewAnonymousType(sourceType, fieldNames);
                var exprNew = ExpressionBuilder.BuildNew(fieldNames, sourceType, anonType, arg);
                selector = Expression.Lambda(exprNew, new ParameterExpression[] { arg });
                type = anonType.AsType();
            }

            var enumarableType = typeof(Queryable);
            var selectMethod = enumarableType.GetMethods()
                 .Where(m => m.Name == "Select" && m.IsGenericMethodDefinition)
                 .Where(m =>
                 {
                     var parameters = m.GetParameters().ToList();
                     return parameters.Count == 2;
                 }).First().MakeGenericMethod(targetType, type);
            if (SubInstruction != null)
            {
                return new KeyValuePair<string, Expression>(Name, Expression.Call(typeof(Queryable), "Select", new Type[] { targetType, type }, subExpr, selector));
            }

            return new KeyValuePair<string, Expression>(Name, Expression.Call(typeof(Queryable), "Select", new Type[] { targetType, type }, Expression.Constant(source), selector));
        }
    }
}
