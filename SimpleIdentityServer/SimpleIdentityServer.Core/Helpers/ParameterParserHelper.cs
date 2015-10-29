using System;
using System.Collections.Generic;
using System.Linq;

using SimpleIdentityServer.Core.Parameters;

namespace SimpleIdentityServer.Core.Helpers
{
    public interface IParameterParserHelper
    {
        List<PromptParameter> ParsePromptParameters(string parameter);

        List<string> ParseScopeParameters(string scope);
    }

    public class ParameterParserHelper : IParameterParserHelper
    {
        /// <summary>
        /// Parse the parameter and returns a list of prompt parameter.
        /// </summary>
        /// <param name="parameter">List of prompts separate by whitespace</param>
        /// <returns>List of prompts.</returns>
        public List<PromptParameter> ParsePromptParameters(string parameter)
        {
            var defaultResult = new List<PromptParameter>
            {
                PromptParameter.none
            };

            var promptValues = Enum.GetNames(typeof(PromptParameter));
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return defaultResult;
            }

            var prompts = parameter.Split(' ')
                .Where(c => !string.IsNullOrWhiteSpace(c) && promptValues.Contains(c))
                .Select(c => (PromptParameter)Enum.Parse(typeof(PromptParameter), c))
                .ToList();
            if (prompts == null || !prompts.Any())
            {
                prompts = defaultResult;
            }

            return prompts;
        }

        public List<string> ParseScopeParameters(string scope)
        {
            return scope.Split(' ').ToList();
        }
    }
}
