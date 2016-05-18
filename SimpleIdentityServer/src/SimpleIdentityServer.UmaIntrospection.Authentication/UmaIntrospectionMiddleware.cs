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
using Microsoft.Extensions.OptionsModel;
using System;

namespace SimpleIdentityServer.UmaIntrospection.Authentication
{
    internal class UmaIntrospectionMiddleware<TOptions> where TOptions : UmaIntrospectionOptions, new()
    {
        private readonly RequestDelegate _next;

        private readonly IApplicationBuilder _app;

        private readonly UmaIntrospectionOptions _options;

        #region Constructor

        public UmaIntrospectionMiddleware(
            RequestDelegate next,
            IApplicationBuilder app,
            IOptions<TOptions> options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {

            }
        }


        #endregion

        #region Public methods



        #endregion
    }
}
