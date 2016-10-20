#region copyright
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleIdentityServer.Scim.Core.Parsers
{
    public enum ComparisonOperators
    {
        // Equal
        eq,
        // Not equal
        ne,
        // Contains
        co,
        // Starts with
        sw,
        // Ends with
        ew,
        // Present (has value)
        pr,
        // Greater than
        gt,
        // Greater than or equal to
        ge,
        // Less than
        lt,
        // Less than or equal to
        le
    }

    public enum LogicalOperators
    {
        // Logical and
        and,
        // Logical or
        or,
        // Not function
        not
    }

    public class AttributePath
    {
        public string Name { get; set; }
        public AttributePath Next { get; set; }
        public Filter ValueFilter { get; set; }
    }

    public class AttributeExpression : Expression
    {
        public AttributePath Path { get; set; }
    }

    public class CompAttributeExpression : Expression
    {
        public AttributePath Path { get; set; }
        public ComparisonOperators Operator { get; set; }
        public string Value { get; set; }
    }
    
    public class LogicalExpression : Expression
    {
        public Expression AttributeLeft {  get; set; }
        public Expression AttributeRight { get; set; }
        public LogicalOperators Operator { get; set; }
    }

    public class Expression
    {
    }

    public class Filter
    {
        public Expression Expression { get; set; }

        public void Evaluate()
        {
        }
    }

    public interface IFilterParser
    {
        Filter Parse(string path);
    }

    public class FilterParser : IFilterParser
    {
        private static IEnumerable<char> _openSign = new[]
        {
            '(',
            '['
        };
        private static IEnumerable<char> _closeSign = new[]
        {
            ')',
            ']'
        };

        public Filter Parse(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            return ParseFilter(path);
        }

        private static Filter ParseFilter(string filter)
        {
            var regex = new Regex("[ ]{2,}", RegexOptions.None);
            filter = regex.Replace(filter, " ");
            var result = new Filter();
            var strBuilder = new StringBuilder();
            var attrs = SplitFilter(filter);
            var isLogicalAttribute = attrs.Any(a => IsLogicalOperand(a));
            if (attrs.Any(a => IsLogicalOperand(a)))
            {
                result.Expression = GetLogicalExpression(attrs);
            }
            else if (attrs.Any(a => IsComparisonOperand(a)))
            {
                result.Expression = GetAttributeExpression(attrs);
            }
            else if (attrs.Count() == 1)
            {
                result.Expression = new AttributeExpression
                {
                    Path = GetPath(attrs.First())
                };
            }

            return result;
        }

        private static Expression GetLogicalExpression(IEnumerable<string> parameters)
        {
            var indexes = FindAllIndexes(parameters, new[] { "and", "or" });
            Expression leftAttr = null;
            int start = 0;
            int i = 0;
            foreach (var index in indexes)
            {
                LogicalExpression logicalExpression = null;
                var lastIndex = i == indexes.Count() - 1 ? parameters.Count() - 1 : 
                    indexes.ElementAt(i + 1) - 1 - index;
                var op = (LogicalOperators)Enum.Parse(typeof(LogicalOperators), parameters.ElementAt(index));
                var rightOperand = parameters.Skip(index + 1).Take(lastIndex);
                if (leftAttr == null)
                {
                    var leftOperand = parameters.Skip(start).Take(index - start);
                    logicalExpression = new LogicalExpression
                    {
                        AttributeLeft = GetAttributeExpression(leftOperand),
                        AttributeRight = GetAttributeExpression(rightOperand),
                        Operator = op
                    };
                }
                else
                {
                    logicalExpression = new LogicalExpression
                    {
                        AttributeLeft = leftAttr,
                        AttributeRight = GetAttributeExpression(rightOperand),
                        Operator = op
                    };
                }

                leftAttr = logicalExpression;
                i++;
            }

            return leftAttr;
        }

        private static Expression GetAttributeExpression(IEnumerable<string> parameters)
        {
            ComparisonOperators op = (ComparisonOperators)Enum.Parse(typeof(ComparisonOperators), parameters.ElementAt(1));
            return new CompAttributeExpression
            {
                Operator = op,
                Value = parameters.ElementAt(2),
                Path = GetPath(parameters.First())
            };
        }

        public static AttributePath GetPath(string path)
        {
            var values = SplitPath(path);
            AttributePath result = null;
            AttributePath tmp = null;
            foreach(var value in values)
            {
                var startIndex = value.IndexOf('[');
                var endIndex = value.IndexOf(']');
                var attr = new AttributePath
                {
                    Name = value
                };

                if (startIndex > -1 && endIndex > -1)
                {
                    attr.Name = value.Substring(0, startIndex);
                    var filter = value.Substring(startIndex + 1, endIndex - startIndex - 1);
                    attr.ValueFilter = ParseFilter(filter);
                }

                if (result == null)
                {
                    result = attr;
                    tmp = result;
                    continue;
                }

                tmp.Next = attr;
                tmp = attr;
            }

            return result;
        }

        private static bool IsLogicalOperand(string parameter)
        {
            return new[] { "and", "or" }.Contains(parameter);
        }

        private static bool IsComparisonOperand(string parameter)
        {
            return new[] { "eq" }.Contains(parameter);
        }

        private static IEnumerable<string> SplitFilter(string filter)
        {
            return SplitStr(filter, ' ');
        }

        private static IEnumerable<string> SplitPath(string path)
        {
            return SplitStr(path, '.');
        }

        private static IEnumerable<string> SplitStr(string str, char separator)
        {
            int i = 0,
                level = 0;
            var strBuilder = new StringBuilder();
            var attrs = new List<string>();
            foreach (var character in str)
            {
                i++;
                if (_closeSign.Contains(character))
                {
                    level--;
                }

                // 1. Add the character.
                if (i == str.Length ||
                    (level == 0 && character != separator) ||
                    (level > 0))
                {
                    strBuilder.Append(character);
                }

                // 2. Add string.
                if (level == 0 && (character == separator || i == str.Length))
                {
                    attrs.Add(strBuilder.ToString().TrimStart('(').TrimEnd(')'));
                    strBuilder.Clear();
                    continue;
                }

                if (_openSign.Contains(character))
                {
                    level++;
                }
            }

            return attrs;
        }

        private static IEnumerable<int> FindAllIndexes(IEnumerable<string> parameters, IEnumerable<string> lst)
        {
            var result = new List<int>();
            int index = 0;
            foreach(var parameter in parameters)
            {
                if (lst.Contains(parameter))
                {
                    result.Add(index);
                }

                index++;
            }

            return result.OrderBy(d => d);
        }
    }
}
