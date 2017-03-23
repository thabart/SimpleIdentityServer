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
using System.Text.RegularExpressions;

namespace SimpleIdentityServer.EventStore.EF.Parsers
{
    public class WhereInstruction : BaseInstruction
    {
        public const string Name = "where";

        public WhereInstruction()
        {
        }

        public override KeyValuePair<string, Expression>? GetExpression<TSource>(Type sourceType, ParameterExpression rootParameter, IEnumerable<TSource> source)
        {
            const string expression = "(\\S* (eq|neq|co|sw|ew) (('|\")(\\S| )*('|\")|(\\S)*)( and | or ){0,1})+";
            var match = Regex.Match(Parameter, expression);
            if (string.IsNullOrWhiteSpace(match.Value) || match.Value != Parameter)
            {
                throw new InvalidOperationException($"the parameter {Parameter} is not correct");
            }

            Type sourceQueryableType = null;
            Expression subExpression = null;
            if (SubInstruction != null)
            {
                var subExpr = SubInstruction.GetExpression(sourceType, rootParameter, source);
                subExpression = subExpr.Value.Value;
                sourceType = subExpression.Type.GetGenericArguments().First();
            }

            var conditions = Regex.Split(Parameter, " *(and|or) *");
            var arg = Expression.Parameter(sourceType, "x");
            Expression binaryExpr = null;
            if (conditions.Count() != 1)
            {
                Expression rightOperand = null;
                ExpressionType type = ExpressionType.AndAlso;
                for (var i = conditions.Count() - 1; i >= 0; i--)
                {
                    var str = conditions.ElementAt(i);
                    if ((i + 1) % 2 == 0)
                    {
                        if (str == "or")
                        {
                            type = ExpressionType.Or;
                        }
                        else
                        {
                            type = ExpressionType.AndAlso;
                        }
                    }
                    else
                    {
                        var expr = BuildCondition(str, arg);
                        if (rightOperand == null)
                        {
                            rightOperand = expr;
                        }
                        else
                        {
                            rightOperand = Expression.MakeBinary(type, expr, rightOperand);
                        }
                    }
                }

                binaryExpr = rightOperand;
            }
            else
            {
                binaryExpr = BuildCondition(conditions.First(), arg);
            }

            sourceQueryableType = typeof(IQueryable<>).MakeGenericType(sourceType);
            var selector = Expression.Lambda(binaryExpr, new ParameterExpression[] { arg });
            var enumarableType = typeof(Queryable);
            var genericMethod = enumarableType.GetMethods()
                 .Where(m => m.Name == "Where" && m.IsGenericMethodDefinition)
                 .Where(m => m.GetParameters().Count() == 2).First().MakeGenericMethod(sourceType);
            if (subExpression != null)
            {
                var call = Expression.Call(genericMethod, subExpression, selector);
                return new KeyValuePair<string, Expression>(Name, call);
            }

            return new KeyValuePair<string, Expression>(Name, Expression.Call(genericMethod, rootParameter, selector));
        }

        private static Expression BuildCondition(string str, ParameterExpression arg)
        {
            var p = Regex.Split(str, " *(eq|neq|co|sw|ew) *");
            if (p.Count() != 3)
            {
                throw new InvalidOperationException($"the condition {str} is not correct");
            }

            var prop = p.Last().TrimStart('\'', '"').TrimEnd('\'', '"');
            var property = Expression.Property(arg, p.First());
            // Not equal
            if (p.ElementAt(1) == "neq")
            {
                return Expression.NotEqual(property, Expression.Constant(prop));
            }

            // Contains
            if (p.ElementAt(1) == "co")
            {
                var propInfo = (PropertyInfo)property.Member;
                var methodInfo = propInfo.PropertyType.GetMethod("Contains", new[] { typeof(string) } );
                return Expression.Call(property, methodInfo, Expression.Constant(prop));
            }

            // Starts with
            if (p.ElementAt(1) == "sw")
            {
                var propInfo = (PropertyInfo)property.Member;
                var methodInfo = propInfo.PropertyType.GetMethod("StartsWith", new[] { typeof(string), typeof(StringComparison) });
                return Expression.Call(property, methodInfo, Expression.Constant(prop), Expression.Constant(StringComparison.CurrentCultureIgnoreCase));
            }

            // Ends with
            if (p.ElementAt(1) == "ew")
            {
                var propInfo = (PropertyInfo)property.Member;
                var methodInfo = propInfo.PropertyType.GetMethod("EndsWith", new[] { typeof(string), typeof(StringComparison) });
                return Expression.Call(property, methodInfo, Expression.Constant(prop), Expression.Constant(StringComparison.CurrentCultureIgnoreCase));
            }

            return Expression.Equal(property, Expression.Constant(prop));
        }
    }
}
