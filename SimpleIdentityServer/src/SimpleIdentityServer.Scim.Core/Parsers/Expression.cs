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

using SimpleIdentityServer.Scim.Common.DTOs;
using SimpleIdentityServer.Scim.Core.Models;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Core.Parsers
{

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
            if (representationAttrs == null)
            {
                throw new ArgumentNullException(nameof(representationAttrs));
            }

            return EvaluateRepresentation(representationAttrs);
        }

        public IEnumerable<SchemaAttributeResponse> Evaluate(SchemaResponse schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            return Evaluate(schema.Attributes);
        }

        public IEnumerable<SchemaAttributeResponse> Evaluate(IEnumerable<SchemaAttributeResponse> schemaAttrs)
        {
            if (schemaAttrs == null)
            {
                throw new ArgumentNullException(nameof(schemaAttrs));
            }

            return EvaluateSchema(schemaAttrs);
        }

        protected abstract IEnumerable<RepresentationAttribute> EvaluateRepresentation(IEnumerable<RepresentationAttribute> representationAttrs);
        protected abstract IEnumerable<SchemaAttributeResponse> EvaluateSchema(IEnumerable<SchemaAttributeResponse> schemaAttrs);
    }
}
