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
using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Core.Parsers
{
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

        public IEnumerable<SchemaAttributeResponse> Evaluate(SchemaResponse schema)
        {
            if (schema == null)
            {
                return null;
            }

            return Expression.Evaluate(schema);
        }

        public IEnumerable<RepresentationAttribute> Evaluate(IEnumerable<RepresentationAttribute> representations)
        {
            if (Expression == null)
            {
                return null;
            }

            return Expression.Evaluate(representations);
        }

        public IEnumerable<SchemaAttributeResponse> Evaluate(IEnumerable<SchemaAttributeResponse> schemaAttrs)
        {
            if (Expression == null)
            {
                return null;
            }

            return Expression.Evaluate(schemaAttrs);
        }
    }
}
