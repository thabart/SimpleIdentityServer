﻿#region copyright
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

using SimpleIdentityServer.Authentication.Common.Authentication;
using SimpleIdentityServer.UserInformation.Authentication.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
#if NETSTANDARD1_6
using curl_sharp;
#endif

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
            nullAuthenticationBuilder.UseMiddleware<NullAuthenticationMiddleware>(Options.Create(nullAuthenticationOptions));
            nullAuthenticationBuilder.Run(ctx => next(ctx));
            _nullAuthenticationNext = nullAuthenticationBuilder.Build();
        }

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
                            accessToken).ConfigureAwait(false);

                        if (userInformationResponse != null)
                        {
                            context.User = CreateClaimPrincipal(userInformationResponse);
                        }
                    }
                }
            }

            await _nullAuthenticationNext(context).ConfigureAwait(false);
        }

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
#if NET46
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            requestMessage.Headers.Add(AuthorizationName, string.Format("Bearer {0}", token));
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
#else
            var argument = string.Format("-H \"Accept: application/json\" -H \"Authorization: Bearer {0}\" -X GET {1}",
                token, url);
            try
            {
                var content = await CurlClient.Curl(argument).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
            }
            catch (Exception)
            {
                return null;
            }
#endif
        }

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
    }
}
