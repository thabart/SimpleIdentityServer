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

using System;
using System.Linq;
using SimpleIdentityServer.Uma.Core.Api.Parameters;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;

namespace SimpleIdentityServer.Uma.Core.Api.ResourceSetController.Actions
{
    public interface IAddResourceSetAction
    {
        string Execute(AddResouceSetParameter addResourceSetParameter);
    }
    
    public class AddResourceSetAction : IAddResourceSetAction
    {
        private readonly IResourceSetRepository _resourceSetRepository;
    
        #region Constructor
        
        public AddResourceSetAction(IResourceSetRepository resourceSetRepository)
        {
            _resourceSetRepository = resourceSetRepository;
        }
        
        #endregion
    
        public string Execute(AddResouceSetParameter addResourceSetParameter)
        {
            if (addResourceSetParameter == null)
            {
                throw new ArgumentNullException(nameof(addResourceSetParameter));
            }
            
            // 1. Check parameters
            if (String.IsNullOrWhiteSpace(addResourceSetParameter.Name))
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode, 
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "name"));
            }
            
            if (addResourceSetParameter.Scopes == null ||
                !addResourceSetParameter.Scopes.Any())
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, "scopes"));    
            }
            
            if (!String.IsNullOrWhiteSpace(addResourceSetParameter.IconUri) &&
                !Uri.IsWellFormedUriString(addResourceSetParameter.IconUri, UriKind.Absolute))
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, addResourceSetParameter.IconUri));
            }
            
            if (!String.IsNullOrWhiteSpace(addResourceSetParameter.Uri) &&
                !Uri.IsWellFormedUriString(addResourceSetParameter.Uri, UriKind.Absolute))
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, addResourceSetParameter.Uri));    
            }
            
            // 2. Store resource set
            var resourceSet = new ResourceSet
            {
                Name = addResourceSetParameter.Name,
                Uri = addResourceSetParameter.Uri,
                Type = addResourceSetParameter.Type,
                Scopes = addResourceSetParameter.Scopes,
                IconUri = addResourceSetParameter.IconUri
            };
            
            var result = _resourceSetRepository.Insert(resourceSet);
            return result.Id;
        }
    }
}