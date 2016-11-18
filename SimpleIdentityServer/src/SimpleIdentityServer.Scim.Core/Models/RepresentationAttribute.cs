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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Scim.Core.Models
{
    public class RepresentationAttribute : ICloneable, IComparable
    {
        public RepresentationAttribute(SchemaAttributeResponse schemaAttribute)
        {
            SchemaAttribute = schemaAttribute;
        }

        public SchemaAttributeResponse SchemaAttribute { get; private set; }

        public RepresentationAttribute Parent { get; set; }

        public string FullPath
        {
            get
            {
                return GetFullPath();
            }
        }

        public object Clone()
        {
            return CloneObj();
        }

        protected virtual object CloneObj()
        {
            return new RepresentationAttribute(SchemaAttribute);
        }

        private string GetFullPath()
        {
            var parents = new List<RepresentationAttribute>();
            var names = new List<string>();
            if (SchemaAttribute != null)
            {
                names.Add(SchemaAttribute.Name);
            }

            GetParents(this, parents);
            parents.Reverse();
            var parentNames = names.Concat(parents.Where(p => p.SchemaAttribute != null).Select(p => p.SchemaAttribute.Name));
            return string.Join(".", parentNames);
        }

        private IEnumerable<RepresentationAttribute> GetParents(RepresentationAttribute representation, IEnumerable<RepresentationAttribute> parents)
        {
            if (representation.Parent == null)
            {
                return parents;
            }

            parents = parents.Concat(new[] { representation });
            return GetParents(representation.Parent, parents);
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            var representation = obj as RepresentationAttribute;
            if (representation == null)
            {
                return 1;
            }

            return CompareTo(representation);
        }

        public virtual string GetSerializedValue()
        {
            return string.Empty;
        }

        public virtual bool SetValue(RepresentationAttribute attr)
        {
            return false;
        }

        protected virtual int CompareTo(RepresentationAttribute attr)
        {
            return 0;
        }
    }

    public class SingularRepresentationAttribute<T> : RepresentationAttribute
    {
        public SingularRepresentationAttribute(SchemaAttributeResponse type, T value): base(type)
        {
            Value = value;
        }

        public T Value { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var representationObj = obj as SingularRepresentationAttribute<T>;
            if (representationObj == null)
            {
                return false;
            }

            return representationObj.Value.Equals(Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string GetSerializedValue()
        {
            return JsonConvert.SerializeObject(Value);
        }

        public override bool SetValue(RepresentationAttribute attr)
        {
            if (attr == null || attr.SchemaAttribute == null || SchemaAttribute == null)
            {
                return false;
            }

            var target = attr as SingularRepresentationAttribute<T>;
            if (target == null)
            {
                return false;
            }
            
            Value = target.Value;
            return true;
        }

        protected override object CloneObj()
        {
            return new SingularRepresentationAttribute<T>(SchemaAttribute, Value);
        }

        protected override int CompareTo(RepresentationAttribute attr)
        {
            var singular = attr as SingularRepresentationAttribute<T>;
            if (singular == null)
            {
                return 1;
            }

            return CompareTo(singular.Value);
        }

        private int CompareTo(T target)
        {
            // TODO : Compare the IEnumerable !!
            if (SchemaAttribute.MultiValued)
            {
                return 0;
            }

            switch (SchemaAttribute.Type)
            {
                case Constants.SchemaAttributeTypes.String:
                    var ss = Value as string;
                    var ts = target as string;
                    return ss.CompareTo(ts);
                case Constants.SchemaAttributeTypes.Boolean:
                    var sb = bool.Parse(Value as string);
                    var tb = bool.Parse(target as string);
                    return sb.CompareTo(tb);
                case Constants.SchemaAttributeTypes.Integer:
                    var si = int.Parse(Value as string);
                    var ti = int.Parse(target as string);
                    return si.CompareTo(ti);
                case Constants.SchemaAttributeTypes.Decimal:
                    var sd = decimal.Parse(Value as string);
                    var td = decimal.Parse(target as string);
                    return sd.CompareTo(td);
                case Constants.SchemaAttributeTypes.DateTime:
                    var sdt = DateTime.Parse(Value as string);
                    var tdt = DateTime.Parse(target as string);
                    return sdt.CompareTo(tdt);
            }

            return -1;
        }
    }

    public class ComplexRepresentationAttribute : RepresentationAttribute
    {
        public ComplexRepresentationAttribute(SchemaAttributeResponse type): base(type)
        {
        }

        public IEnumerable<RepresentationAttribute> Values { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var complexRepresentation = obj as ComplexRepresentationAttribute;
            if (complexRepresentation == null)
            {
                return false;
            }

            var result = Values.All(v => complexRepresentation.Values.Contains(v));
            return result;
        }

        public override int GetHashCode()
        {
            int result = 0;
            foreach(var value in Values)
            {
                result = result ^ value.GetHashCode();
            }

            return result;
        }

        protected override object CloneObj()
        {
            var newValues = new List<RepresentationAttribute>();
            if (Values != null)
            {
                foreach (var value in Values)
                {
                    newValues.Add((RepresentationAttribute)value.Clone());
                }
            }

            return new ComplexRepresentationAttribute(SchemaAttribute)
            {
                Values = newValues
            };
        }

        protected override int CompareTo(RepresentationAttribute attr)
        {
            var complex = attr as ComplexRepresentationAttribute;
            if (complex == null || complex.Values == null)
            {
                return 1;
            }

            if (Values == null)
            {
                return -1;
            }

            var sourcePrimary = Values.FirstOrDefault(p => p.SchemaAttribute != null && p.SchemaAttribute.Name == Constants.MultiValueAttributeNames.Primary);
            var targetPrimary = complex.Values.FirstOrDefault(p => p.SchemaAttribute != null && p.SchemaAttribute.Name == Constants.MultiValueAttributeNames.Primary);
            if (sourcePrimary == null || targetPrimary == null)
            {
                return 0;
            }

            return sourcePrimary.CompareTo(targetPrimary);
        }
    }
}
