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

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SimpleIdentityServer.Vse.Identifiers;
using System;
using System.ComponentModel.Design;

namespace SimpleIdentityServer.Vse
{
    internal sealed class GenerateProxyCommand
    {
        #region Public static fields

        public static GenerateProxyCommand Instance
        {
            get;
            private set;
        }

        #endregion

        #region Fields

        private readonly Options _options;

        private IServiceProvider ServiceProvider
        {
            get
            {
                return _options.Package;
            }
        }

        #endregion

        #region Constructor

        private GenerateProxyCommand(Options options)
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

            this._options = options;
            var commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(Guids.MyMenuGroup, PkgCmdIds.GenerateProxyCommand);
                var menuItem = new MenuCommand(ShowWindow, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        #endregion

        #region Public static methods

        public static void Initialize(Options options)
        {
            Instance = new GenerateProxyCommand(options);
        }

        #endregion

        #region Private methods

        private void ShowWindow(object sender, EventArgs e)
        {
            var window = _options.Package.FindToolWindow(typeof(GenerateProxyWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            ((GenerateProxyWindow)window).Initialize(_options);
            var windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        #endregion
    }
}
