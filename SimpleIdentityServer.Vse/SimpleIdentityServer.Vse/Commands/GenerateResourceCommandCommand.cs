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
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SimpleIdentityServer.Vse.Identifiers;

namespace SimpleIdentityServer.Vse
{
    internal sealed class GenerateResourceCommandCommand
    {
        #region Public static properties

        public static GenerateResourceCommandCommand Instance
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

        private GenerateResourceCommandCommand(Options options)
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
                var menuCommandID = new CommandID(Guids.MyMenuGroup, PkgCmdIds.GenerateResourceCommand);
                var menuItem = new MenuCommand(this.ShowToolWindow, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        #endregion

        #region Public static methods

        public static void Initialize(Options options)
        {
            Instance = new GenerateResourceCommandCommand(options);
        }

        #endregion

        #region Private methods

        private void ShowToolWindow(object sender, EventArgs e)
        {
            var window = _options.Package.FindToolWindow(typeof(GenerateResourceWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            ((GenerateResourceWindow)window).Initialize(_options);
            var windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        #endregion
    }
}
