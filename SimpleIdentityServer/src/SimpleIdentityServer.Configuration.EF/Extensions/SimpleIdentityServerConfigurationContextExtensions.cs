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

using SimpleIdentityServer.Configuration.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Configuration.EF.Extensions
{
    public static class SimpleIdentityServerConfigurationContextExtensions
    {
        #region Public static methods

        public static void EnsureSeedData(this SimpleIdentityServerConfigurationContext context)
        {
            InsertAuthenticationProviders(context);
            context.SaveChanges();
        }

        #endregion

        #region Private static methods

        private static void InsertAuthenticationProviders(SimpleIdentityServerConfigurationContext context)
        {
            if (!context.AuthenticationProviders.Any())
            {
                context.AuthenticationProviders.AddRange(new[]
                {
                    new AuthenticationProvider
                    {
                        IsEnabled = true,
                        Name = "Facebook",
                        Options = new List<Option>
                        {
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientId",
                                Value = "569242033233529"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientSecret",
                                Value = "12e0f33817634c0a650c0121d05e53eb"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "Scope",
                                Value = "email"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "Scope",
                                Value = "public_profile"
                            }
                        }
                    },
                    new AuthenticationProvider
                    {
                        IsEnabled = true,
                        Name = "Microsoft",
                        Options = new List<Option>
                        {
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientId",
                                Value = "59b073ec-cd5e-4616-bf6d-7a78312fc4a8"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientSecret",
                                Value = "8NHDwaWR9pqPzQQKchNOeza"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "Scope",
                                Value = "openid"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "Scope",
                                Value = "profile"
                            }
                        }
                    },
                    new AuthenticationProvider
                    {
                        IsEnabled = false,
                        Name = "ADFS",
                        Options = new List<Option>
                        {
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientId",
                                Value = "clientid"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientSecret",
                                Value = "clientsecret"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "RelyingParty",
                                Value = "rp"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClaimsIssuer",
                                Value = "url://ci"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "AdfsAuthorizationEndPoint",
                                Value = "https://adfs.mycompany.com/adfs/oauth2/authorize/"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "AdfsTokenEndPoint",
                                Value = "https://adfs.mycompany.com/adfs/oauth2/token/"
                            }
                        }
                    },
					new AuthenticationProvider
                    {
                        IsEnabled = true,
                        Name = "EID",
                        Options = new List<Option>
                        {
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "IdPEndpoint",
                                Value = "https://www.e-contract.be/eid-idp/protocol/ws-federation/auth-ident"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "Realm",
                                Value = "urn://idserver"
                            }
                        }
                    },
                    new AuthenticationProvider
                    {
                        IsEnabled = true,
                        Name = "Google",
                        Options = new List<Option>
                        {
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientId",
                                Value = "636126285787-755svbmi6j75t54e00lk58fi1t1qs4c6.apps.googleusercontent.com"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientSecret",
                                Value = "l-3B1I0hGNc-0S4NSdkIw2yE"
                            }
                        }
                    },
                    new AuthenticationProvider
                    {
                        IsEnabled = true,
                        Name = "Twitter",
                        Options = new List<Option>
                        {
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientId",
                                Value = "g8so3MHsMdklZ8NHau1VfOcXB"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientSecret",
                                Value = "Flxp1fR3XLVj2gsVpwSigJRy80sBdNUPum3CZUkeyyKwbzvlJz"
                            }
                        }
                    }
                });
            }
        }

        #endregion
    }
}
