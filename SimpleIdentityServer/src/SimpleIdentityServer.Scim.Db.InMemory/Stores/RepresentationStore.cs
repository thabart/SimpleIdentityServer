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

using SimpleIdentityServer.Scim.Core.Models;
using SimpleIdentityServer.Scim.Core.Stores;
using System.Linq;
using System.Collections.Generic;

namespace SimpleIdentityServer.Scim.Db.InMemory.Stores
{
    public class RepresentationStore : IRepresentationStore
    {
        public bool AddRepresentation(Representation representation)
        {
            Storage.Instance().Representations.Add(representation);
            return true;
        }

        public Representation GetRepresentation(string id)
        {
            return Storage.Instance().Representations.FirstOrDefault(r => r.Id == id);
        }

        public IEnumerable<Representation> GetRepresentations(string resourceType)
        {
            return Storage.Instance().Representations.Where(r => r.ResourceType == resourceType);
        }

        public bool RemoveRepresentation(Representation representation)
        {
            var representations = Storage.Instance().Representations;
            var record = representations.FirstOrDefault(r => r.Id == representation.Id);
            if (record == null)
            {
                return false;
            }

            representations.Remove(record);
            return true;
        }

        public bool UpdateRepresentation(Representation representation)
        {
            var representations = Storage.Instance().Representations;
            representations.Remove(representations.First(r => r.Id == representation.Id));
            representations.Add(representation);
            return true;
        }
    }
}
