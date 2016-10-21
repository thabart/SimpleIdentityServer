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

using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Core.Models;
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

    public class AttributePath
    {
        public string Name { get; set; }
        public AttributePath Next { get; set; }
        public Filter ValueFilter { get; set; }

        public string Evaluate(JToken jObj)
        {
            var token = jObj.SelectToken(Name);
            if (token == null)
            {
                return null;
            }

            if (Next != null)
            {
                return Next.Evaluate(token);
            }

            return token.ToString();
        }

        public IEnumerable<RepresentationAttribute> Evaluate(IEnumerable<RepresentationAttribute> representationAttrs)
        {
            var representations = representationAttrs.Where(r => r.SchemaAttribute.Name == Name);
            if (!representations.Any())
            {
                return null;
            }

            if ((ValueFilter != null && representations.Any(r => !r.SchemaAttribute.MultiValued)) ||
                (Next != null && representations.Any(r => r.SchemaAttribute.Type != Constants.SchemaAttributeTypes.Complex)))
            {
                return null;
            }

            if (ValueFilter != null || Next != null)
            {
                var subAttrs = new List<RepresentationAttribute>();
                foreach (var representation in representations)
                {
                    var complexAttr = representation as ComplexRepresentationAttribute;
                    if (complexAttr == null || complexAttr.Values == null)
                    {
                        continue;
                    }

                    if (ValueFilter != null)
                    {
                        complexAttr.Values = ValueFilter.Evaluate(complexAttr.Values);
                    }

                    if (Next != null)
                    {
                        subAttrs.AddRange(Next.Evaluate(complexAttr.Values));
                    }
                }

                if (subAttrs.Any())
                {
                    return subAttrs;
                }
            }

            return representations;
        }
    }

    public class AttributeExpression : Expression
    {
        public AttributePath Path { get; set; }

        protected override IEnumerable<RepresentationAttribute> EvaluateRepresentation(IEnumerable<RepresentationAttribute> representationAttrs)
        {
            return Path.Evaluate(representationAttrs);
        }
    }

    public class CompAttributeExpression : Expression
    {
        public AttributePath Path { get; set; }
        public ComparisonOperators Operator { get; set; }
        public string Value { get; set; }

        protected override IEnumerable<RepresentationAttribute> EvaluateRepresentation(IEnumerable<RepresentationAttribute> representationAttrs)
        {
            var result = new List<RepresentationAttribute>();
            foreach(var representationAttr in representationAttrs)
            {
                var attr = Path.Evaluate(new[] { representationAttr });
                if (attr.Where(a => Compare(a, Operator, Value)).Any())
                {
                    result.Add(representationAttr);
                }
            }

            return result;
        }

        private static bool Compare(RepresentationAttribute attr, ComparisonOperators op, string value)
        {
            switch(attr.SchemaAttribute.Type)
            {
                case Constants.SchemaAttributeTypes.String:
                    return Equals(attr, op, value);
                case Constants.SchemaAttributeTypes.Boolean:
                    var b = false;
                    if (!bool.TryParse(value, out b))
                    {
                        return false;
                    }

                    return Equals(attr, op, b);
                case Constants.SchemaAttributeTypes.DateTime:
                    DateTime d = default(DateTime);
                    if (!DateTime.TryParse(value, out d))
                    {
                        return false;
                    }

                    return Equals(attr, op, d);
                case Constants.SchemaAttributeTypes.Decimal:
                    decimal dec;
                    if (!decimal.TryParse(value, out dec))
                    {
                        return false;
                    }

                    return Equals(attr, op, dec);
                case Constants.SchemaAttributeTypes.Integer:
                    int i;
                    if (!int.TryParse(value, out i))
                    {
                        return false;
                    }

                    return Equals(attr, op, i);
            }

            return false;
        }

        private static bool Equals(RepresentationAttribute attr, ComparisonOperators op, string value)
        {
            if (attr.SchemaAttribute.MultiValued)
            {
                var enumAttr = attr as SingularRepresentationAttribute<IEnumerable<string>>;
                if (enumAttr == null)
                {
                    return false;
                }

                switch(op)
                {
                    case ComparisonOperators.co:
                        return enumAttr.Value.Contains(value);
                }

                return false;
            }

            var attrValue = attr as SingularRepresentationAttribute<string>;
            if (attrValue == null)
            {
                return false;
            }

            switch(op)
            {
                case ComparisonOperators.eq:
                    return attrValue.Value.Equals(value);
                case ComparisonOperators.ne:
                    return !attrValue.Value.Equals(value);
                case ComparisonOperators.sw:
                    return attrValue.Value.StartsWith(value);
                case ComparisonOperators.ew:
                    return attrValue.Value.EndsWith(value);
                case ComparisonOperators.pr:
                    return !string.IsNullOrWhiteSpace(attrValue.Value);
                case ComparisonOperators.gt:
                    return attrValue.Value.CompareTo(value) > 0;
                case ComparisonOperators.ge:
                    return attrValue.Value.CompareTo(value) > 0 || attrValue.Value.Equals(value);
                case ComparisonOperators.lt:
                    return attrValue.Value.CompareTo(value) < 0;
                case ComparisonOperators.le:
                    return attrValue.Value.CompareTo(value) < 0 || attrValue.Value.Equals(value);
            }

            return false;
        }

        private static bool Equals(RepresentationAttribute attr, ComparisonOperators op, bool value)
        {
            if (attr.SchemaAttribute.MultiValued)
            {
                var enumAttr = attr as SingularRepresentationAttribute<IEnumerable<bool>>;
                if (enumAttr == null)
                {
                    return false;
                }

                switch (op)
                {
                    case ComparisonOperators.co:
                        return enumAttr.Value.Contains(value);
                }

                return false;
            }

            var attrValue = attr as SingularRepresentationAttribute<bool>;
            if (attrValue == null)
            {
                return false;
            }

            switch (op)
            {
                case ComparisonOperators.eq:
                    return attrValue.Value.Equals(value);
                case ComparisonOperators.ne:
                    return !attrValue.Value.Equals(value);
                case ComparisonOperators.pr:
                    return true;
            }

            return false;
        }

        private static bool Equals(RepresentationAttribute attr, ComparisonOperators op, DateTime value)
        {
            if (attr.SchemaAttribute.MultiValued)
            {
                var enumAttr = attr as SingularRepresentationAttribute<IEnumerable<DateTime>>;
                if (enumAttr == null)
                {
                    return false;
                }

                switch (op)
                {
                    case ComparisonOperators.co:
                        return enumAttr.Value.Contains(value);
                }

                return false;
            }

            var attrValue = attr as SingularRepresentationAttribute<DateTime>;
            if (attrValue == null)
            {
                return false;
            }

            switch (op)
            {
                case ComparisonOperators.eq:
                    return attrValue.Value.Equals(value);
                case ComparisonOperators.ne:
                    return !attrValue.Value.Equals(value);
                case ComparisonOperators.pr:
                    return attrValue.Value != null;
                case ComparisonOperators.gt:
                    return attrValue.Value.CompareTo(value) > 0;
                case ComparisonOperators.ge:
                    return attrValue.Value.CompareTo(value) > 0 || attrValue.Value.Equals(value);
                case ComparisonOperators.lt:
                    return attrValue.Value.CompareTo(value) < 0;
                case ComparisonOperators.le:
                    return attrValue.Value.CompareTo(value) < 0 || attrValue.Value.Equals(value);
            }

            return false;
        }

        private static bool Equals(RepresentationAttribute attr, ComparisonOperators op, decimal value)
        {
            if (attr.SchemaAttribute.MultiValued)
            {
                var enumAttr = attr as SingularRepresentationAttribute<IEnumerable<decimal>>;
                if (enumAttr == null)
                {
                    return false;
                }

                switch (op)
                {
                    case ComparisonOperators.co:
                        return enumAttr.Value.Contains(value);
                }

                return false;
            }

            var attrValue = attr as SingularRepresentationAttribute<decimal>;
            if (attrValue == null)
            {
                return false;
            }

            switch (op)
            {
                case ComparisonOperators.eq:
                    return attrValue.Value.Equals(value);
                case ComparisonOperators.ne:
                    return !attrValue.Value.Equals(value);
                case ComparisonOperators.pr:
                    return true;
                case ComparisonOperators.gt:
                    return attrValue.Value.CompareTo(value) > 0;
                case ComparisonOperators.ge:
                    return attrValue.Value.CompareTo(value) > 0 || attrValue.Value.Equals(value);
                case ComparisonOperators.lt:
                    return attrValue.Value.CompareTo(value) < 0;
                case ComparisonOperators.le:
                    return attrValue.Value.CompareTo(value) < 0 || attrValue.Value.Equals(value);
            }

            return false;
        }

        private static bool Equals(RepresentationAttribute attr, ComparisonOperators op, int value)
        {
            if (attr.SchemaAttribute.MultiValued)
            {
                var enumAttr = attr as SingularRepresentationAttribute<IEnumerable<int>>;
                if (enumAttr == null)
                {
                    return false;
                }

                switch (op)
                {
                    case ComparisonOperators.co:
                        return enumAttr.Value.Contains(value);
                }

                return false;
            }

            var attrValue = attr as SingularRepresentationAttribute<int>;
            if (attrValue == null)
            {
                return false;
            }

            switch (op)
            {
                case ComparisonOperators.eq:
                    return attrValue.Value.Equals(value);
                case ComparisonOperators.ne:
                    return !attrValue.Value.Equals(value);
                case ComparisonOperators.pr:
                    return true;
                case ComparisonOperators.gt:
                    return attrValue.Value.CompareTo(value) > 0;
                case ComparisonOperators.ge:
                    return attrValue.Value.CompareTo(value) > 0 || attrValue.Value.Equals(value);
                case ComparisonOperators.lt:
                    return attrValue.Value.CompareTo(value) < 0;
                case ComparisonOperators.le:
                    return attrValue.Value.CompareTo(value) < 0 || attrValue.Value.Equals(value);
            }

            return false;
        }
    }
    
    public class LogicalExpression : Expression
    {
        public Expression AttributeLeft {  get; set; }
        public Expression AttributeRight { get; set; }
        public LogicalOperators Operator { get; set; }

        protected override IEnumerable<RepresentationAttribute> EvaluateRepresentation(IEnumerable<RepresentationAttribute> representationAttrs)
        {
            var result = new List<RepresentationAttribute>();
            foreach(var attr in representationAttrs)
            {
                var left = AttributeLeft.Evaluate(new[] { attr });
                var right = AttributeRight.Evaluate(new[] { attr });
                bool isNot = !Operator.HasFlag(LogicalOperators.not);
                if ((Operator.HasFlag(LogicalOperators.and) && left != null && right != null && left.Any() && right.Any()) == isNot
                   || (Operator.HasFlag(LogicalOperators.or) && (left != null && left.Any()) || (right != null && right.Any())) == isNot)
                {
                    result.Add(attr);
                }
            }

            return result;
        }
    }

    public abstract class Expression
    {
        public IEnumerable<RepresentationAttribute> Evaluate(Representation representation)
        {
            if (representation == null)
            {
                throw new ArgumentNullException(nameof(representation));
            }

            return Evaluate(representation.Attributes);
        }

        public IEnumerable<RepresentationAttribute> Evaluate(IEnumerable<RepresentationAttribute> representationAttrs)
        {
            if (representationAttrs  == null)
            {
                throw new ArgumentNullException(nameof(representationAttrs));
            }

            return EvaluateRepresentation(representationAttrs);
        }

        protected abstract IEnumerable<RepresentationAttribute> EvaluateRepresentation(IEnumerable<RepresentationAttribute> representationAttrs);
    }

    public class Filter
    {
        public Expression Expression { get; set; }

        public IEnumerable<RepresentationAttribute> Evaluate(Representation representation)
        {
            if (Expression == null)
            {
                return null;
            }

            return Expression.Evaluate(representation);
        }

        public IEnumerable<RepresentationAttribute> Evaluate(IEnumerable<RepresentationAttribute> representations)
        {
            if (Expression == null)
            {
                return null;
            }

            return Expression.Evaluate(representations);
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

                var rightOperand = parameters.Skip(index + 1).Take(lastIndex);
                var attrRight = getAttributeExpression(rightOperand);
                if (leftAttr == null)
                {
                    var leftOperand = parameters.Skip(start).Take(index - start);
                    var attrLeft = getAttributeExpression(leftOperand);
                    logicalExpression = new LogicalExpression
                    {
                        AttributeLeft = attrLeft,
                        AttributeRight = attrRight,
                        Operator = op
                    };
                }
                else
                {
                    logicalExpression = new LogicalExpression
                    {
                        AttributeLeft = leftAttr,
                        AttributeRight = attrRight,
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
                Value =  op != ComparisonOperators.pr ? parameters.ElementAt(2) : null,
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
