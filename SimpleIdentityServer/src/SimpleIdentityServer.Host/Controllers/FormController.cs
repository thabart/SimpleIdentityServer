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
using SimpleIdentityServer.Host.ViewModels;

namespace SimpleIdentityServer.Api.Controllers
{
    public class FormController : Controller
    {
        public ActionResult Index(dynamic parameters)
        {
            var queryStringValue = Request.QueryString.Value;            
            var queryString = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(queryStringValue);
            var idToken = queryString[Core.Constants.StandardAuthorizationResponseNames.IdTokenName];
            var accessToken = queryString[Core.Constants.StandardAuthorizationResponseNames.AccessTokenName];
            var authorizationCode = queryString[Core.Constants.StandardAuthorizationResponseNames.AuthorizationCodeName];
            var state = queryString[Core.Constants.StandardAuthorizationResponseNames.StateName];
            var redirectUri = queryString["redirect_uri"];
            return View(new FormViewModel
            {
                AccessToken = accessToken,
                AuthorizationCode = authorizationCode,
                IdToken = idToken,
                RedirectUri = redirectUri,
                State = state
            });
        }
    }
}