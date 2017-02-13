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

namespace SimpleIdentityServer.EventStore.EF.Parsers
{
    public class SqlInterpreter
    {
        private readonly BaseInstruction _instruction;

        public SqlInterpreter(BaseInstruction instruction)
        {
            _instruction = instruction;
        }

        public IEnumerable<dynamic> Execute<TSource>(IQueryable<TSource> records)
        {
            if (records == null)
            {
                throw new ArgumentNullException(nameof(records));
            }

            var finalSelectArg = Expression.Parameter(typeof(IQueryable<TSource>), "f");
            var expr = _instruction.GetExpression(typeof(TSource), finalSelectArg, records);
            var finalSelectRequestBody = Expression.Lambda(expr.Value.Value, new ParameterExpression[] { finalSelectArg });
            return (IEnumerable<dynamic>)finalSelectRequestBody.Compile().DynamicInvoke(records);
        }
    }
}
