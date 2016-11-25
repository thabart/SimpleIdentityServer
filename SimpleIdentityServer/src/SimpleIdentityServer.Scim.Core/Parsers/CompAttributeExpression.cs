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

using System.Linq;
using SimpleIdentityServer.Scim.Core.Models;
using System;
using System.Collections.Generic;
using SimpleIdentityServer.Scim.Common.DTOs;

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

    public class CompAttributeExpression : Expression
    {
        public AttributePath Path { get; set; }
        public ComparisonOperators Operator { get; set; }
        public string Value { get; set; }

        protected override IEnumerable<RepresentationAttribute> EvaluateRepresentation(IEnumerable<RepresentationAttribute> representationAttrs)
        {
            var result = new List<RepresentationAttribute>();
            foreach (var representationAttr in representationAttrs)
            {
                var attr = Path.Evaluate(new[] { representationAttr });
                if (attr.Where(a => Compare(a, Operator, Value)).Any())
                {
                    result.Add(representationAttr);
                }
            }

            return result;
        }

        protected override IEnumerable<SchemaAttributeResponse> EvaluateSchema(IEnumerable<SchemaAttributeResponse> schemaAttrs)
        {
            // The schema cannot be evaluated.
            return null;
        }

        private static bool Compare(RepresentationAttribute attr, ComparisonOperators op, string value)
        {
            switch (attr.SchemaAttribute.Type)
            {
                case Common.Constants.SchemaAttributeTypes.String:
                    return Equals(attr, op, value);
                case Common.Constants.SchemaAttributeTypes.Boolean:
                    var b = false;
                    if (!bool.TryParse(value, out b))
                    {
                        return false;
                    }

                    return Equals(attr, op, b);
                case Common.Constants.SchemaAttributeTypes.DateTime:
                    DateTime d = default(DateTime);
                    if (!DateTime.TryParse(value, out d))
                    {
                        return false;
                    }

                    return Equals(attr, op, d);
                case Common.Constants.SchemaAttributeTypes.Decimal:
                    decimal dec;
                    if (!decimal.TryParse(value, out dec))
                    {
                        return false;
                    }

                    return Equals(attr, op, dec);
                case Common.Constants.SchemaAttributeTypes.Integer:
                    int i;
                    if (!int.TryParse(value, out i))
                    {
                        return false;
                    }

                    return Equals(attr, op, i);
                case Common.Constants.SchemaAttributeTypes.Complex:
                    var complexAttr = attr as ComplexRepresentationAttribute;
                    if (complexAttr == null)
                    {
                        return false;
                    }

                    return Equals(complexAttr, op, value);
            }

            return false;
        }

        private static bool Equals(ComplexRepresentationAttribute attr, ComparisonOperators op, string value)
        {
            return attr.Values != null && op == ComparisonOperators.pr;
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

                switch (op)
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

            switch (op)
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
}
