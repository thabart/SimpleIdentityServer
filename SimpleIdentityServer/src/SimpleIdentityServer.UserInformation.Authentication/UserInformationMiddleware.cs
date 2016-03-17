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
using Microsoft.AspNet.Http;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json;
using SimpleIdentityServer.Authentication.Common.Authentication;
using SimpleIdentityServer.UserInformation.Authentication.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.UserInformation.Authentication
{
    public class UserInformationMiddleware<TOptions> where TOptions : UserInformationOptions, new()
    {
        private const string AuthorizationName = "Authorization";

        private const string BearerName = "Bearer";

        private readonly UserInformationOptions _options;

        private readonly HttpClient _httpClient;

        private readonly RequestDelegate _next;

        private readonly RequestDelegate _nullAuthenticationNext;

        #region Constructor

        public UserInformationMiddleware(
            RequestDelegate next,
            IApplicationBuilder app,
            IOptions<TOptions> options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _next = next;
            _options = options.Value;
            _httpClient = new HttpClient(_options.BackChannelHttpHandler ?? new HttpClientHandler());

            var nullAuthenticationBuilder = app.New();
            var nullAuthenticationOptions = new NullAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            };
            nullAuthenticationBuilder.UseMiddleware<NullAuthenticationMiddleware>(nullAuthenticationOptions);
            nullAuthenticationBuilder.Run(ctx => next(ctx));
            _nullAuthenticationNext = nullAuthenticationBuilder.Build();
        }

        #endregion

        #region Public methods

        public async Task Invoke(HttpContext context)
        {
            // 1. Try to authenticate the user against the introspection endpoint
            var headers = context.Request.Headers;
            if (headers.ContainsKey(AuthorizationName))
            {
                var authorizationValues = headers[AuthorizationName];
                var authorizationValue = authorizationValues.FirstOrDefault();
                if (authorizationValue != null)
                {
                    var accessToken = GetAccessToken(authorizationValue);
                    if (!string.IsNullOrWhiteSpace(accessToken))
                    {
                        var userInformationResponse = await GetUserInformationResponse(
                            _options,
                            accessToken);

                        if (userInformationResponse != null)
                        {
                            context.User = CreateClaimPrincipal(userInformationResponse);
                        }
                    }
                }
            }

            await _nullAuthenticationNext(context);
        }

        #endregion

        #region Private methods

        private string GetAccessToken(string authorizationValue)
        {
            var splittedAuthorizationValue = authorizationValue.Split(' ');
            if (splittedAuthorizationValue.Count() == 2 &&
                splittedAuthorizationValue[0].Equals(BearerName, StringComparison.CurrentCultureIgnoreCase))
            {
                return splittedAuthorizationValue[1];
            }

            return string.Empty;
        }

        private async Task<Dictionary<string, string>> GetUserInformationResponse(
            UserInformationOptions userInformationOptions,
            string token)
        {
            Uri uri = null;
            var url = userInformationOptions.UserInformationEndPoint;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                throw new ArgumentException(ErrorDescriptions.TheUserInfoEndPointIsNotAWellFormedUrl);
            }
            
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            requestMessage.Headers.Add(AuthorizationName, string.Format("Bearer {0}", token));
            var response = await _httpClient.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
        }

        #endregion

        #region Private static methods

        private static ClaimsPrincipal CreateClaimPrincipal(Dictionary<string, string> userInformationResponse)
        {
            var claims = new List<Claim>();
            foreach(var claim in userInformationResponse)
            {
                claims.Add(new Claim(claim.Key, claim.Value));
            }

            var claimsIdentity = new ClaimsIdentity(claims, "UserInformation");
            return new ClaimsPrincipal(claimsIdentity);
        }

        #endregion
    }
}
