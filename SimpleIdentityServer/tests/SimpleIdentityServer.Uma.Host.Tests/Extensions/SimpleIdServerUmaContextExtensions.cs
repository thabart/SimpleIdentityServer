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

using SimpleIdentityServer.Uma.EF;
using SimpleIdentityServer.Uma.EF.Models;
using System.Diagnostics;
using System.Linq;

namespace SimpleIdentityServer.Uma.Host.Tests.Extensions
{
    public static class SimpleIdServerUmaContextExtensions
    {
        public static void EnsureSeedData(this SimpleIdServerUmaContext context)
        {
            InsertResources(context);
            try
            {
                context.SaveChanges();
            }
            catch
            {
                Trace.WriteLine("already exists");
            }
        }

        private static void InsertResources(SimpleIdServerUmaContext context)
        {
            if (!context.ResourceSets.Any())
            {
                context.ResourceSets.AddRange(new[]
                {
                    new ResourceSet
                    {
                        Id = "bad180b5-4a96-422d-a088-c71a9f7c7afc",
                        Name = "Resources"
                    },
                    new ResourceSet
                    {
                        Id = "67c50eac-23ef-41f0-899c-dffc03add961",
                        Name = "Apis"
                    }
                });
            }
        }
    }
}
