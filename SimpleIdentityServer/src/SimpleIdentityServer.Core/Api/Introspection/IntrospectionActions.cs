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

using SimpleIdentityServer.Core.Api.Introspection.Actions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Introspection
{
    public interface IIntrospectionActions
    {
        Task<IntrospectionResult> PostIntrospection(
            IntrospectionParameter introspectionParameter,
            AuthenticationHeaderValue authenticationHeaderValue);
    }

    public class IntrospectionActions : IIntrospectionActions
    {
        private readonly IPostIntrospectionAction _postIntrospectionAction;

        public IntrospectionActions(IPostIntrospectionAction postIntrospectionAction)
        {
            _postIntrospectionAction = postIntrospectionAction;
        }

        public Task<IntrospectionResult> PostIntrospection(IntrospectionParameter introspectionParameter, AuthenticationHeaderValue authenticationHeaderValue)
        {
            if (introspectionParameter == null)
            {
                throw new ArgumentNullException(nameof(introspectionParameter));
            }

            return _postIntrospectionAction.Execute(introspectionParameter, authenticationHeaderValue);
        }
    }
}
