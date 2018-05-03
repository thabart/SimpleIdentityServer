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

using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.EF.Extensions;
using SimpleIdentityServer.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.EF.Repositories
{
    public sealed class TranslationRepository : ITranslationRepository
    {         
        private readonly SimpleIdentityServerContext _context;

        private readonly IManagerEventSource _managerEventSource;

        public TranslationRepository(
            SimpleIdentityServerContext context,
            IManagerEventSource managerEventSource)
        {
            _context = context;
            _managerEventSource = managerEventSource;
        }

        public async Task<Translation> GetAsync(string languageTag, string code)
        {
            var result = await _context.Translations.FirstOrDefaultAsync(t => t.Code == code && t.LanguageTag == languageTag).ConfigureAwait(false);
            if (result == null)
            {
                return null;
            }

            return result.ToDomain();
        }

        public async Task<ICollection<Translation>> GetAsync(string languageTag)
        {
            return await _context.Translations.Where(t => t.LanguageTag == languageTag)
                .Select(r => r.ToDomain()).ToListAsync().ConfigureAwait(false);
        }

        public async Task<ICollection<string>> GetLanguageTagsAsync()
        {
            return await _context.Translations.Select(t => t.LanguageTag).Distinct().ToListAsync().ConfigureAwait(false);
        }
    }
}
