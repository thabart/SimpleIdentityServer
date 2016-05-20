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

using SimpleIdentityServer.Uma.Core.Code;
using System;
using System.IO;
using System.Linq;

namespace SimpleIdentityServer.Uma.Core.Api.CodeSampleController.Actions
{
    public interface IGetFrontendCodeAction
    {
        MemoryStream Execute(string languageCode);
    }

    internal class GetFrontendCodeAction : IGetFrontendCodeAction
    {
        private readonly ICodeProvider _codeProvider;

        #region Constructor

        public GetFrontendCodeAction(ICodeProvider codeProvider)
        {
            _codeProvider = codeProvider;
        }

        #endregion

        #region Public methods

        public MemoryStream Execute(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                throw new ArgumentNullException(nameof(languageCode));
            }

            if (!Constants.MappingLanguageToCodes.Values.Contains(languageCode))
            {
                throw new ArgumentException("the language is not supported");
            }

            var language = Constants.MappingLanguageToCodes.First(m => m.Value == languageCode).Key;
            return _codeProvider.GetFiles(language, Code.TypeCode.Frontend);
        }

        #endregion
    }
}
