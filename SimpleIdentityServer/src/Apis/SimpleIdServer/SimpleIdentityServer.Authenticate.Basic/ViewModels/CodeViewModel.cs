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

using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace SimpleIdentityServer.Authenticate.Basic.ViewModels
{
    public class CodeViewModel
    {
        public const string RESEND_ACTION = "resend";
        public const string SUBMIT_ACTION = "submit";

        public string Code { get; set; }
        public string AuthRequestCode { get; set; }
        public string ClaimName { get; set; }
        public string ClaimValue { get; set; }
        public string Action { get; set; }

        public void Validate(ModelStateDictionary modelState)
        {
            if (modelState == null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }

            if (Action == RESEND_ACTION)
            {
                if (string.IsNullOrWhiteSpace(ClaimValue))
                {
                    modelState.AddModelError("ClaimValue", "The claim must be specified");
                }
            }

            if (Action == SUBMIT_ACTION)
            {
                if (string.IsNullOrWhiteSpace(Code))
                {
                    modelState.AddModelError("Code", "The confirmation code must be specified");
                }
            }
        }
    }
}
