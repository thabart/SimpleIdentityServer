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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace SimpleIdentityServer.Vse
{
    [Guid("0d1ee03f-2245-410b-aaec-f56ba0707cc4")]
    public class GenerateProxyWindow : ToolWindowPane
    {
        private Package _package;

        #region Constructor

        public GenerateProxyWindow() : base(null)
        {
            Caption = "Generate security proxy";
            Content = new GenerateProxyWindowControl();
        }

        #endregion

        #region Public methods

        public void Initialize(Options options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.Package == null)
            {
                throw new ArgumentNullException(nameof(options.Package));
            }

            if (options.Dte == null)
            {
                throw new ArgumentNullException(nameof(options.Dte));
            }

            (Content as GenerateProxyWindowControl).Initialize(options);
        }

        #endregion
    }
}
