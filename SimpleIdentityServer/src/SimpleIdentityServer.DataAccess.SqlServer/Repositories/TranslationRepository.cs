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

using System.Collections.Generic;
using System.Linq;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed class TranslationRepository : ITranslationRepository
    {         
        private readonly SimpleIdentityServerContext _context;
        
        public TranslationRepository(SimpleIdentityServerContext context) {
            _context = context;
        }
        public Translation GetTranslationByCode(
            string languageTag,
            string code)
        {
            var result = _context.Translations.FirstOrDefault(t => t.Code == code && t.LanguageTag == languageTag);
            if (result == null)
            {
                return null;
            }

            return result.ToDomain();
        }

        public List<Translation> GetTranslations(string languageTag)
        {
            var result = _context.Translations.Where(t => t.LanguageTag == languageTag).ToList();
            return result.Select(r => r.ToDomain()).ToList();
        }

        public List<string> GetSupportedLanguageTag()
        {
            var result = _context.Translations.Select(t => t.LanguageTag).Distinct();
            return result.ToList();
        }
    }
}
