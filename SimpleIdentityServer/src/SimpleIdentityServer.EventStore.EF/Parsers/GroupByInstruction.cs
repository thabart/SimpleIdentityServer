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

namespace SimpleIdentityServer.EventStore.EF.Parsers
{
    public class GroupByInstruction : BaseInstruction
    {
        private const char _fieldSeparator = '|';
        private const string _onInstruction = "on";
        private const string _aggregateInstruction = "aggregate";
        private static IEnumerable<string> _groupByInstructions = new[]
        {
            _onInstruction,
            _aggregateInstruction
        };

        public const string Name = "groupby";

        public override KeyValuePair<string, Expression>? GetExpression<TSource>(Type sourceType, ParameterExpression rootParameter, IEnumerable<TSource> source)
        {
            // Callback used to generate the group by expression.
            Func<IEnumerable<string>, Type, Type, Expression, MethodCallExpression> getGroupByInst = (names, eType, qType, groupByArg) =>
            {
                var method = qType.GetMethods()
                     .Where(m => m.Name == "GroupBy" && m.IsGenericMethodDefinition)
                     .Where(m => m.GetParameters().ToList().Count == 2).First();
                ParameterExpression arg = Expression.Parameter(eType, "x");
                MethodInfo genericMethod = null;
                LambdaExpression selector = null;
                if (names.Count() == 1)
                {
                    var fieldName = names.First();
                    var propertyInfo = eType.GetProperty(fieldName);
                    MemberExpression keyProperty = Expression.Property(arg, fieldName);
                    var keySelector = Expression.Lambda(keyProperty, new ParameterExpression[] { arg });
                    genericMethod = method.MakeGenericMethod(eType, propertyInfo.PropertyType);
                    selector = Expression.Lambda(keyProperty, new ParameterExpression[] { arg });
                }
                else
                {
                    var sourceTypes = names.Select(n => sourceType.GetProperty(n));
                    var expressions = sourceTypes.Select(s => Expression.Property(arg, s));
                    var anonType = ReflectionHelper.CreateNewAnonymousType(sourceType,  names);
                    var newExpr = Expression.New(anonType.DeclaredConstructors.First(), expressions);
                    selector = Expression.Lambda(newExpr, arg);
                    genericMethod = method.MakeGenericMethod(eType, anonType.AsType());
                }

                return Expression.Call(genericMethod, groupByArg, selector);
            };

            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }

            Type queryableType = typeof(Queryable),
                enumerableType = typeof(Enumerable);

            // 1. Split the value & extract the field names or requests.
            var splitted = Parameter.Split(',');
            var instructions = splitted.Select(s =>
            {
                return InstructionHelper.ExtractInstruction(s);
            }).Where(s => s.HasValue);
            if (instructions.Count() == 0)
            {
                return null;
            }
            if (instructions.Count() != splitted.Count())
            {
                throw new ArgumentException("At least one parameter is not an instruction : <operation>(<parameter>)");
            }

            if (instructions.Any(i => !_groupByInstructions.Contains(i.Value.Key)))
            {
                throw new ArgumentException("At least one instruction is not supported");
            }

            var onInstruction = instructions.FirstOrDefault(i => i.Value.Key == _onInstruction);
            if (onInstruction == null)
            {
                throw new ArgumentException("The 'on' instruction must be specified");
            }

            // r
            var sourceQueryableType = typeof(IQueryable<>).MakeGenericType(sourceType);
            var sourceEnumerableType = typeof(IEnumerable<>).MakeGenericType(sourceType);
            var orderedEnumerableType = typeof(IOrderedEnumerable<>).MakeGenericType(sourceType);
            Expression finalSelectArg;
            if (IsLastRootInstruction())
            {
                finalSelectArg = Expression.Constant(source);
            }
            else
            {
                finalSelectArg = Expression.Parameter(sourceQueryableType, "r");
            }

            var fieldNames = onInstruction.Value.Value.Split(_fieldSeparator);
            var groupByInst = getGroupByInst(fieldNames, sourceType, queryableType, finalSelectArg);
            MethodCallExpression instruction = null;
            var aggregateInstruction = instructions.FirstOrDefault(i => i.Value.Key == _aggregateInstruction);
            if (aggregateInstruction != null)
            {
                var aggregateInstructionVal = InstructionHelper.ExtractAggregateInstruction(aggregateInstruction.Value.Value);
                if (aggregateInstructionVal == null)
                {
                    throw new ArgumentException("the aggregate instruction is not correct");
                }

                var kvp = aggregateInstructionVal.Value;
                var method = queryableType.GetMethods()
                     .Where(m => m.Name == "Select" && m.IsGenericMethodDefinition)
                     .Where(m => m.GetParameters().ToList().Count == 2).First()
                     .MakeGenericMethod(new Type[] { sourceQueryableType, sourceQueryableType });

                var propertyName = kvp.Value;
                var propertyInfo = sourceType.GetProperty(propertyName);
                // o
                var orderArg = Expression.Parameter(sourceType, "o");
                // s
                var selectArg = Expression.Parameter(sourceEnumerableType, "s");
                // b
                var selectFirstArg = Expression.Parameter(sourceEnumerableType, "b");
                // o => o.[Value]
                var keyProperty = Expression.Property(orderArg, propertyName);
                var orderBySelector = Expression.Lambda(keyProperty, new ParameterExpression[] { orderArg });
                // s => s.OrderBy(o => o.[Value])
                MethodCallExpression orderByCall = null;
                if (string.Equals(kvp.Key, "max", StringComparison.CurrentCultureIgnoreCase))
                {
                    orderByCall = Expression.Call(enumerableType, "OrderByDescending", new[] { sourceType, propertyInfo.PropertyType }, selectArg, orderBySelector);
                }
                else
                {
                    orderByCall = Expression.Call(enumerableType, "OrderBy", new[] { sourceType, propertyInfo.PropertyType }, selectArg, orderBySelector);
                }

                var selectBody = Expression.Lambda(orderByCall, new ParameterExpression[] { selectArg });
                var quotedSelectBody = Expression.Quote(selectBody);
                // z.GroupBy(x => x.FirstName).Select(s => s.OrderBy(o => o.BirthDate))
                var selectRequest = Expression.Call(queryableType, "Select", new[] { sourceEnumerableType, orderedEnumerableType }, groupByInst, quotedSelectBody);
                // b => b.First()
                var selectFirstRequest = Expression.Call(enumerableType, "First", new[] { sourceType }, selectFirstArg);
                var selectFirstLambda = Expression.Lambda(selectFirstRequest, new ParameterExpression[] { selectFirstArg });
                instruction = Expression.Call(queryableType, "Select", new[] { sourceEnumerableType, sourceType }, selectRequest, selectFirstLambda);
            }
            else
            {
                instruction = groupByInst;
            }

            return new KeyValuePair<string, Expression>(Name, instruction);
        }
    }
}
