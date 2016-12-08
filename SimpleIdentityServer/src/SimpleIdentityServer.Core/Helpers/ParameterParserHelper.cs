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
using System.Collections.Generic;
using System.Linq;

using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.Core.Helpers
{
    public interface IParameterParserHelper
    {
        /// <summary>
        /// Parse the parameter and returns a list of prompt parameter.
        /// </summary>
        /// <param name="parameter">List of prompts separated by whitespace</param>
        /// <returns>List of prompts.</returns>
        ICollection<PromptParameter> ParsePrompts(string parameter);
        /// <summary>
        /// Parse the parameter and returns a list of response types
        /// </summary>
        /// <param name="parameter">List of response types separated by whitespace</param>
        /// <returns>List of response types</returns>
        ICollection<ResponseType> ParseResponseTypes(string parameter);
        /// <summary>
        /// Parse the parameter and returns a list of scopes.
        /// </summary>
        /// <param name="scope">Parameter to parse.</param>
        /// <returns>list of scopes or null</returns>
        ICollection<string> ParseScopes(string parameter);
        // List<string> ParseScopeParametersAndGetAllScopes(string concatenateListOfScopes);
    }

    public class ParameterParserHelper : IParameterParserHelper
    {
        private readonly IScopeRepository _scopeRepository;

        public ParameterParserHelper(IScopeRepository scopeRepository)
        {
            _scopeRepository = scopeRepository;
        }

        /// <summary>
        /// Parse the parameter and returns a list of prompt parameter.
        /// </summary>
        /// <param name="parameter">List of prompts separated by whitespace</param>
        /// <returns>List of prompts.</returns>
        public ICollection<PromptParameter> ParsePrompts(string parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return new List<PromptParameter>();
            }

            var promptNames = Enum.GetNames(typeof(PromptParameter));

            var prompts = parameter.Split(' ')
                .Where(c => !string.IsNullOrWhiteSpace(c) && promptNames.Contains(c))
                .Select(c => (PromptParameter)Enum.Parse(typeof(PromptParameter), c))
                .ToList();
            return prompts;
        }

        /// <summary>
        /// Parse the parameter and returns a list of response types
        /// </summary>
        /// <param name="parameter">List of response types separated by whitespace</param>
        /// <returns>List of response types</returns>
        public ICollection<ResponseType> ParseResponseTypes(string parameter)
        {
            var responseTypeNames = Enum.GetNames(typeof (ResponseType));
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return new List<ResponseType>();
            }

            var responses = parameter.Split(' ')
                .Where(r => !string.IsNullOrWhiteSpace(r) && responseTypeNames.Contains(r))
                .Select(r => (ResponseType) Enum.Parse(typeof (ResponseType), r))
                .ToList();
            return responses;
        }

        /// <summary>
        /// Parse the parameter and returns a list of scopes.
        /// </summary>
        /// <param name="scope">Parameter to parse.</param>
        /// <returns>list of scopes or null</returns>
        public ICollection<string> ParseScopes(string parameter)
        {
            return string.IsNullOrWhiteSpace(parameter) ? new List<string>() : parameter.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s));
        }

        /*
        public List<string> ParseScopeParametersAndGetAllScopes(string concatenateListOfScopes)
        {
            var result = new List<string>();
            var scopes = ParseScopeParameters(concatenateListOfScopes);
            if (scopes == null || !scopes.Any())
            {
                return result;
            }

            foreach (var scope in scopes)
            {
                var scopeRecord = _scopeRepository.GetScopeByName(scope);
                if (scopeRecord != null)
                {
                    result.Add(scope);
                }
            }

            return result;
        }*/
    }
}
