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

using Microsoft.AspNet.Builder;
using System;

namespace SimpleIdentityServer.Oauth2Instrospection.Authentication
{
    public static class ApplicationBuilderExtensions
    {
        #region Public static methods

        public static IApplicationBuilder UseAuthenticationWithIntrospection(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<Oauth2IntrospectionMiddleware<Oauth2IntrospectionOptions>>();
        }

        public static IApplicationBuilder UseAuthenticationWithIntrospection(this IApplicationBuilder app, Oauth2IntrospectionOptions oauth2IntrospectionOptions)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (oauth2IntrospectionOptions == null)
            {
                throw new ArgumentNullException(nameof(oauth2IntrospectionOptions));
            }
            
            return app.UseMiddleware<Oauth2IntrospectionMiddleware<Oauth2IntrospectionOptions>>(Options.Options.Create(oauth2IntrospectionOptions));
        }    
    
        #endregion
    }
}
