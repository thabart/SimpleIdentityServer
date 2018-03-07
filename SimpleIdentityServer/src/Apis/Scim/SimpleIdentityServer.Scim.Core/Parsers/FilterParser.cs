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
    public interface IFilterParser
    {
        Filter Parse(string path);
        string GetTarget(string path);
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

        private static string _and = Enum.GetName(typeof(LogicalOperators), LogicalOperators.and);
        private static string _or = Enum.GetName(typeof(LogicalOperators), LogicalOperators.or);
        private static string _not = Enum.GetName(typeof(LogicalOperators), LogicalOperators.not);
        private static string _andNot = _and + " " + _not;
        private static string _orNot = _or + " " + _not;

        public Filter Parse(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            return ParseFilter(path);
        }

        public string GetTarget(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            var elts = SplitFilter(path);
            if (elts == null || !elts.Any() || elts.Any(e => IsLogicalOperand(e) || IsComparisonOperand(e)))
            {
                return string.Empty;
            }

            return CleanTarget(elts.First());
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
            Func<IEnumerable<string>, Expression> getAttributeExpression = (s) =>
            {
                if (IsAttributeExpression(s))
                {
                    return GetAttributeExpression(s);
                }

                return ParseFilter(s.First()).Expression;
            };
            var indexes = FindAllIndexes(parameters, new[] { _or, _and, _not, _andNot, _orNot });
            Expression leftAttr = null;
            int start = 0;
            int i = 0;
            foreach (var index in indexes)
            {
                LogicalExpression logicalExpression = null;
                var lastIndex = i == indexes.Count() - 1 ? parameters.Count() - 1 : 
                    indexes.ElementAt(i + 1) - 1 - index;
                LogicalOperators op;
                var opStr = parameters.ElementAt(index);
                // 1. Parse "and not" & "or not".
                if (!Enum.TryParse(opStr, out op))
                {
                    if (opStr == _andNot)
                    {
                        op = LogicalOperators.and | LogicalOperators.not;
                    }

                    if (opStr == _orNot)
                    {
                        op = LogicalOperators.or | LogicalOperators.not;
                    }
                }

                // 2. Construct right operand & logical expression.
                var rightOperand = parameters.Skip(index + 1).Take(lastIndex);
                var attrRight = getAttributeExpression(rightOperand);
                logicalExpression = new LogicalExpression
                {
                    AttributeRight = attrRight,
                    Operator = op
                };

                // 3. Assign left operand.
                // "not" operator doesn't contain left operand.
                if (op != LogicalOperators.not)
                {
                    if (leftAttr == null)
                    {
                        var leftOperand = parameters.Skip(start).Take(index - start);
                        var attrLeft = getAttributeExpression(leftOperand);
                        logicalExpression.AttributeLeft = attrLeft;
                    }
                    else
                    {
                        logicalExpression.AttributeLeft = leftAttr;
                    }
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
                Value =  op != ComparisonOperators.pr ? parameters.ElementAt(2) : null,
                Path = GetPath(parameters.First())
            };
        }

        private static AttributePath GetPath(string path)
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

        private static string CleanTarget(string target)
        {
            var openIndex = target.IndexOf('[');
            var closeIndex = target.IndexOf(']');
            if (openIndex > -1 && closeIndex > -1)
            {
                var start = target.Substring(0, openIndex);
                var end = target.Substring(closeIndex + 1, target.Length  - closeIndex - 1);
                return CleanTarget(start + end);
            }

            if (openIndex > -1 || closeIndex > -1)
            {
                return string.Empty;
            }

            return target;
        }

        private static bool IsAttributeExpression(IEnumerable<string> parameters)
        {
            return parameters != null && parameters.Any() && (parameters.Count() == 2 || parameters.Count() == 3);
        }

        private static bool IsLogicalOperand(string parameter)
        {
            return parameter == _and ||
                parameter == _or ||
                parameter == _not ||
                parameter == _andNot ||
                parameter == _orNot;
        }

        private static bool IsComparisonOperand(string parameter)
        {
            return Enum.GetNames(typeof(ComparisonOperators)).Contains(parameter);
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

            var indexes = FindAllIndexes(attrs, new[] { _not });
            foreach (var index in indexes)
            {
                if (index == 0)
                {
                    continue;
                }

                if (new[] { _and, _or }.Contains(attrs[index - 1]))
                {
                    attrs[index - 1] = attrs[index - 1] + " " + attrs[index];
                    attrs.RemoveAt(index);
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
