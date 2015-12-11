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

using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Configuration;
using System.Linq;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.Core.Translation
{
    public interface ITranslationManager
    {
        Dictionary<string, string> GetTranslations(
            string concatenateListOfCodeLanguages,
            List<string> translationCodes);
    }

    public class TranslationManager : ITranslationManager
    {
        private readonly ISimpleIdentityServerConfigurator _configurator;

        private readonly ITranslationRepository _translationRepository;

        public TranslationManager(
            ISimpleIdentityServerConfigurator configurator,
            ITranslationRepository translationRepository)
        {
            _configurator = configurator;
            _translationRepository = translationRepository;
        }

        /// <summary>
        /// Get the translation by order of preferrence.
        /// </summary>
        /// <param name="concatenateListOfCodeLanguages"></param>
        /// <param name="translationCodes"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetTranslations(
            string concatenateListOfCodeLanguages,
            List<string> translationCodes)
        {
            var preferredLanguage = GetPreferredLanguage(concatenateListOfCodeLanguages);
            var result = new Dictionary<string, string>();
            foreach(var translationCode in translationCodes)
            {
                var record = _translationRepository.GetTranslationByCode(preferredLanguage, translationCode);
                if (record != null)
                {
                    result.Add(record.Code, record.Value);
                } else
                {
                    result.Add(translationCode, string.Format("[{0}]", translationCode));
                }
            }

            return result;
        }

        private string GetPreferredLanguage(string concatenateListOfCodeLanguages)
        {
            if (string.IsNullOrWhiteSpace(concatenateListOfCodeLanguages))
            {
                return _configurator.DefaultLanguage();
            }

            var listOfCodeLanguages = concatenateListOfCodeLanguages.Split(' ');
            var supportedCodeLanguages = _translationRepository.GetSupportedLanguageTag();
            if (listOfCodeLanguages == null || !listOfCodeLanguages.Any() ||
                supportedCodeLanguages == null || !supportedCodeLanguages.Any())
            {
                return _configurator.DefaultLanguage();
            }

            foreach (var codeLanguage in listOfCodeLanguages)
            {
                if (supportedCodeLanguages.Contains(codeLanguage))
                {
                    return codeLanguage;
                }
            }

            return _configurator.DefaultLanguage();
        }
    }
}
