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
    public class OrderByInstruction : BaseInstruction
    {
        public const string Name = "orderby";

        public override KeyValuePair<string, Expression>? GetExpression(Type sourceType, ParameterExpression rootParameter)
        {
            var propertyInfo = sourceType.GetProperty(Parameter);
            ParameterExpression arg = Expression.Parameter(sourceType, "x");
            MemberExpression property = Expression.Property(arg, Parameter);
            var selector = Expression.Lambda(property, new ParameterExpression[] { arg });
            var enumarableType = typeof(Queryable);
            var method = enumarableType.GetMethods()
                 .Where(m => m.Name == "OrderBy" && m.IsGenericMethodDefinition)
                 .Where(m =>
                 {
                     var parameters = m.GetParameters().ToList();
                     return parameters.Count == 2;
                 }).Single().MakeGenericMethod(sourceType, propertyInfo.PropertyType);
            if (SubInstruction != null)
            {
                var subExpr = SubInstruction.GetExpression(sourceType, rootParameter);
                var call = Expression.Call(method, subExpr.Value.Value, selector);
                return new KeyValuePair<string, Expression>(Name, call);
            }

            return new KeyValuePair<string, Expression>(Name, Expression.Call(method, rootParameter, selector));
        }
    }
}
