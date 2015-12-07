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
        List<PromptParameter> ParsePromptParameters(string parameter);

        /// <summary>
        /// Parse the parameter and returns a list of response types
        /// </summary>
        /// <param name="parameter">List of response types separated by whitespace</param>
        /// <returns>List of response types</returns>
        List<ResponseType> ParseResponseType(string parameter);

        List<string> ParseScopeParameters(string scope);

        List<string> ParseScopeParametersAndGetAllScopes(string concatenateListOfScopes);
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
        public List<PromptParameter> ParsePromptParameters(string parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return null;
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
        public List<ResponseType> ParseResponseType(string parameter)
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

        public List<string> ParseScopeParameters(string scope)
        {
            return string.IsNullOrWhiteSpace(scope) ? new List<string>() :
                scope.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        }

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
        }
    }
}
