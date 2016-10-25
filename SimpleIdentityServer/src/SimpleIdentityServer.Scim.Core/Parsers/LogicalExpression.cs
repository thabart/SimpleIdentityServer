﻿#region copyright
// Copyright 2015 Habart Thierry
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

using System.Linq;
using SimpleIdentityServer.Scim.Core.Models;
using System.Collections.Generic;
using System;

namespace SimpleIdentityServer.Scim.Core.Parsers
{
    [Flags]
    public enum LogicalOperators
    {
        // Logical and
        and = 1,
        // Logical or
        or = 2,
        // Not function
        not = 4
    }

    public class LogicalExpression : Expression
    {
        public Expression AttributeLeft { get; set; }
        public Expression AttributeRight { get; set; }
        public LogicalOperators Operator { get; set; }

        protected override IEnumerable<RepresentationAttribute> EvaluateRepresentation(IEnumerable<RepresentationAttribute> representationAttrs)
        {
            var result = new List<RepresentationAttribute>();
            foreach (var attr in representationAttrs)
            {
                var right = AttributeRight.Evaluate(new[] { attr });
                // 1. Not operator doesn't contain left operator.
                if (!Operator.HasFlag(LogicalOperators.and) && !Operator.HasFlag(LogicalOperators.or))
                {
                    if (Operator.HasFlag(LogicalOperators.not) && !right.Any())
                    {
                        result.Add(attr);
                    }
                }
                else
                {
                    bool isNot = !Operator.HasFlag(LogicalOperators.not);
                    var left = AttributeLeft.Evaluate(new[] { attr });
                    if ((Operator.HasFlag(LogicalOperators.and) && left != null && right != null && left.Any() && right.Any()) == isNot
                       || (Operator.HasFlag(LogicalOperators.or) && (left != null && left.Any()) || (right != null && right.Any())) == isNot)
                    {
                        result.Add(attr);
                    }
                }
            }

            return result;
        }
    }
}