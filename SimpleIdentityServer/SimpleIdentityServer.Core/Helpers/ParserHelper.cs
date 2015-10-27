using SimpleIdentityServer.Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Core.Helpers
{
    public static class ParserHelper
    {
        /// <summary>
        /// Parse the parameter and returns a list of prompt parameter.
        /// </summary>
        /// <param name="promptParameter">List of prompts separate by whitespace</param>
        /// <returns>List of prompts.</returns>
        public static List<PromptParameter> ParsePromptParameters(string parameter)
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
    }
}
