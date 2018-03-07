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

using SimpleIdentityServer.EventStore.EF.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SimpleIdentityServer.EventStore.EF.Parsers
{
    public class OrderByInstruction : BaseInstruction
    {
        public const string Name = "orderby";
        private const string _onInstruction = "on";
        private const string _orderInstruction = "order";
        private static IEnumerable<string> _orderByInstructions = new[]
        {
            _onInstruction,
            _orderInstruction
        };

        public override KeyValuePair<string, Expression>? GetExpression<TSource>(Type sourceType, ParameterExpression rootParameter, IEnumerable<TSource> source)
        {
            // 1. Parse the instruction.
            var fieldNames = Parameter.Split(',');
            var instructions = fieldNames.Select(s =>
            {
                return InstructionHelper.ExtractInstruction(s);
            }).Where(s => s.HasValue);
            if (instructions.Count() == 0)
            {
                throw new ArgumentException("the parameter cannot be empty");
            }

            if (instructions.Any(i => !_orderByInstructions.Contains(i.Value.Key)))
            {
                throw new ArgumentException("At least one instruction is not supported");
            }
            
            var onInstruction = instructions.FirstOrDefault(i => i.Value.Key == _onInstruction);
            var orderInstruction = instructions.FirstOrDefault(i => i.Value.Key == _orderInstruction);
            if (onInstruction == null)
            {
                throw new ArgumentException("The 'on' instruction must be specified");
            }

            var fnName = "OrderBy";
            if (orderInstruction != null && orderInstruction.Value.Value == "desc")
            {
                fnName = "OrderByDescending";
            }

            // 2. Construct the selector.
            ParameterExpression arg = Expression.Parameter(sourceType, "x");
            MemberExpression property = Expression.Property(arg, onInstruction.Value.Value);
            var selector = Expression.Lambda(property, new ParameterExpression[] { arg });
            
            // 3. Call the order function and return the result.
            var enumarableType = typeof(Queryable);
            var method = enumarableType.GetMethods()
                 .Where(m => m.Name == fnName && m.IsGenericMethodDefinition)
                 .Where(m =>
                 {
                     var parameters = m.GetParameters().ToList();
                     return parameters.Count == 2;
                 }).Single().MakeGenericMethod(sourceType, property.Type);
            if (SubInstruction != null)
            {
                var subExpr = SubInstruction.GetExpression(sourceType, rootParameter, source);
                var call = Expression.Call(method, subExpr.Value.Value, selector);
                return new KeyValuePair<string, Expression>(Name, call);
            }

            return new KeyValuePair<string, Expression>(Name, Expression.Call(method, rootParameter, selector));
        }
    }
}
