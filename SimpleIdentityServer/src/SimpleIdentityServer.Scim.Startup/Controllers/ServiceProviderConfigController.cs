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

using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Scim.Core.DTOs;
using SimpleIdentityServer.Scim.Core.Models;
using System;

namespace SimpleIdentityServer.Scim.Startup.Controllers
{
    [Route(Constants.RoutePaths.ServiceProviderConfigController)]
    public class ServiceProviderConfigController : Controller
    {
        #region Public methods

        [HttpGet]
        public ActionResult Get()
        {
            var result = new ServiceProviderConfigResponse
            {
                Schemas = new[] { "urn:ietf:params:scim:schemas:core:2.0:ServiceProviderConfig" },
                DocumentationUri = "http://example.com/help/scim.html",
                Patch = new PatchResponse
                {
                    Supported = true,
                },
                Bulk = new BulkResponse
                {
                    Supported = true,
                    MaxOperations = 1000,
                    MaxPayloadSize = 1048576
                },
                Filter = new FilterResponse
                {
                    Supported = true,
                    MaxResults = 200
                },
                ChangePassword = new ChangePasswordResponse
                {
                    Supported = true
                },
                Sort = new SortResponse
                {
                    Supported = true
                },
                Etag = new EtagResponse
                {
                    Supported = true
                },
                AuthenticationSchemes = new[]
                {
                    new AuthenticationSchemeResponse
                    {
                        Name = "OAUTH Bearer token",
                        Description = "Authentication scheme using the OAuth Bearer Token Standard",
                        SpecUri = "http://www.rfc-editor.org/info/rfc6750",
                        DocumentationUri = "http://example.com/help/oauth.html",
                        Type = "oauthbearertoken",
                        Primary = true
                    },
                    new AuthenticationSchemeResponse
                    {
                        Name = "HTTP Basic",
                        Description = "Authentication scheme using the HTTP Basic Standard",
                        SpecUri = "http://www.rfc-editor.org/info/rfc2617",
                        DocumentationUri = "http://example.com/help/httpBasic.html",
                        Type = "httpbasic"
                    }
                },
                Meta = new MetaResponse
                {
                    Location = "https://example.com/v2/ServiceProviderConfig",
                    ResourceType = "ServiceProviderConfig",
                    Created = DateTime.Now,
                    LastModified = DateTime.Now,
                    Version = "1"
                }
            };

            return new OkObjectResult(result);
        }

        #endregion
    }
}
