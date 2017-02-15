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

namespace SimpleIdentityServer.EventStore.EF.Parsers
{
    public class InnerJoinInstruction : BaseInstruction
    {
        private const string innerKey = "inner";
        private const string outerKey = "outer";
        private readonly IEnumerable<string> parameters = new List<string>
        {
            innerKey,
            outerKey
        };

        public const string Name = "join";

        public InnerJoinInstruction()
        {
        }

        public override KeyValuePair<string, Expression>? GetExpression<TSource>(Type sourceType, ParameterExpression rootParameter, IEnumerable<TSource> source)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            // 1. Split the value & extract the field names or requests.
            var splitted = Parameter.Split(',');
            var instructions = splitted.Select(s =>
            {
                return InstructionHelper.ExtractInstruction(s);
            }).Where(s => s.HasValue);
            if (!parameters.Any(p => instructions.Any(i => i.Value.Key == p)))
            {
                throw new ArgumentException("either inner or outer parameter is not specified");
            }

            // 2. Construct the expression.
            var outer = instructions.First(i => i.Value.Key == outerKey).Value.Value;
            var inner = instructions.First(i => i.Value.Key == innerKey).Value.Value;
            var propertyInfoOuter = sourceType.GetProperty(outer);
            var propertyInfoInner = sourceType.GetProperty(inner);
            var outerArg = Expression.Parameter(sourceType, "x");
            var innerArg = Expression.Parameter(sourceType, "y");
            var propertyOuter = Expression.Property(outerArg, outer);
            var propertyInner = Expression.Property(innerArg, inner);
            var selectorOuter = Expression.Lambda(propertyOuter, new ParameterExpression[] { outerArg });
            var selectorInner = Expression.Lambda(propertyInner, new ParameterExpression[] { innerArg });
            var selectorResult = Expression.Lambda(outerArg, new ParameterExpression[] { outerArg, innerArg });
            var enumarableType = typeof(Queryable);
            var method = enumarableType.GetMethods().Where(m => m.Name == "Join" && m.IsGenericMethodDefinition).Where(m => m.GetParameters().ToList().Count == 5).First();
            var genericMethod = method.MakeGenericMethod(sourceType, sourceType, propertyInfoOuter.PropertyType, sourceType);
            var call = Expression.Call(genericMethod, Expression.Constant(source), Expression.Constant(source), selectorOuter, selectorInner, selectorResult);
            return new KeyValuePair<string, Expression>(Name, call);
        }
    }
}
