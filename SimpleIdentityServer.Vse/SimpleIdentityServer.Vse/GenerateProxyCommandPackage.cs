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
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using SimpleIdentityServer.Vse.Identifiers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SimpleIdentityServer.Vse
{
    public class Options
    {
        public GenerateProxyCommandPackage Package { get; set; }

        public DTE2 Dte2 { get; set; }

        public DTE Dte { get; set; }

        public IComponentModel ComponentModel { get; set; }
    }

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(GenerateProxyWindow))]
    [ProvideToolWindow(typeof(GenerateResourceWindow))]
    [ProvideOptionPage(typeof(OptionsPageGrid), "SimpleIdentityServer", "Configuration", 0, 0, true)]
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class GenerateProxyCommandPackage : Package
    {
        public const string PackageGuidString = Guids.PackageStr;

        #region Constructor

        public GenerateProxyCommandPackage()
        {
        }

        #endregion

        #region Public properties

        public string Url
        {
            get
            {
                var page = (OptionsPageGrid)GetDialogPage(typeof(OptionsPageGrid));
                return page.Url;
            }
        }

        public string WellKnownConfigurationEdp
        {
            get
            {
                var page = (OptionsPageGrid)GetDialogPage(typeof(OptionsPageGrid));
                return page.WellKnownConfigurationEdp;
            }
        }

        #endregion

        #region Package Members

        protected override void Initialize()
        {
            var options = new Options
            {
                Package = this,
                Dte2 = GetService(typeof(DTE)) as DTE2,
                Dte = GetGlobalService(typeof(DTE)) as DTE,
                ComponentModel = (IComponentModel)GetGlobalService(typeof(SComponentModel))
            };
            GenerateProxyCommand.Initialize(options);
            GenerateResourceCommandCommand.Initialize(options);
            base.Initialize();
        }

        #endregion
    }
}
