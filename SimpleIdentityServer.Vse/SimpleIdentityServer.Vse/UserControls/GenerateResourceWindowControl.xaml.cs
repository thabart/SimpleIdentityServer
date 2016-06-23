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

using EnvDTE;
using SimpleIdentityServer.Vse.Extensions;
using SimpleIdentityServer.Vse.Helpers;
using System;
using System.Windows.Controls;

namespace SimpleIdentityServer.Vse
{
    public partial class GenerateResourceWindowControl : UserControl
    {
        private Options _options;

        #region Constructor

        public GenerateResourceWindowControl()
        {
            InitializeComponent();
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

            if (options.Dte2 == null)
            {
                throw new ArgumentNullException(nameof(options.Dte2));
            }

            if (options.ComponentModel == null)
            {
                throw new ArgumentNullException(nameof(options.ComponentModel));
            }

            _options = options;
        }

        #endregion

        #region Event handlers

        private void Refresh(object sender, System.Windows.RoutedEventArgs e)
        {
            var project = _options.Dte2.GetSelectedProject();
            if (project == null)
            {
                return;
            }

            var classes = project.GetClasses();
            foreach (CodeClass codeClass in classes)
            {
                if (codeClass.IsDerivedFrom["Microsoft.AspNetCore.Mvc.Controller"])
                {
                    var functions = VisualStudioHelper.GetAllCodeElementsOfType(codeClass.Members, vsCMElement.vsCMElementFunction, false);
                    foreach (CodeFunction function in functions)
                    {
                        string s = function.Name;
                        var attributes = VisualStudioHelper.GetAllCodeElementsOfType(function.Attributes, vsCMElement.vsCMElementAttribute, false);
                    }
                }
            }
        }

        #endregion
    }
}