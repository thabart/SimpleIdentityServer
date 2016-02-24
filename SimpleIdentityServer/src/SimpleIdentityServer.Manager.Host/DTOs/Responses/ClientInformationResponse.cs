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

using WebApi.Hal;

namespace SimpleIdentityServer.Manager.Host.DTOs.Responses
{
    public class ClientInformationResponse : Representation
    {        
        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client name
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the logo uri
        /// </summary>
        public string LogoUri { get; set; }

        public override string Rel
        {
            get
            {
                return LinkTemplates.Clients.GetClients.Rel;
            }
        }

        public override string Href
        {
            get
            {
                return LinkTemplates.Clients.GetClients.Href;
            }
        }

        protected override void CreateHypermedia()
        {
            Links.Add(LinkTemplates.Clients.GetClient.CreateLink(new { id = ClientId }));
        }
    }
}
