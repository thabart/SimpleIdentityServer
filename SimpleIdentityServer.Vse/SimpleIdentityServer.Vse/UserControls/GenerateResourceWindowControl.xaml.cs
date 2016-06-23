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
using SimpleIdentityServer.UmaManager.Client.DTOs.Requests;
using SimpleIdentityServer.Vse.Extensions;
using SimpleIdentityServer.Vse.Helpers;
using SimpleIdentityServer.Vse.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleIdentityServer.Vse
{
    public partial class GenerateResourceWindowControl : UserControl
    {
        private Options _options;

        private List<string> _httpAttributeFullNames = new List<string>
        {
            "Microsoft.AspNetCore.Mvc.HttpGetAttribute",
            "Microsoft.AspNetCore.Mvc.HttpPutAttribute",
            "Microsoft.AspNetCore.Mvc.HttpPostAttribute",
            "Microsoft.AspNetCore.Mvc.HttpDeleteAttribute"
        };

        private GenerateResourceWindowViewModel _viewModel;

        #region Constructor

        public GenerateResourceWindowControl()
        {
            InitializeComponent();
            Loaded += Load;
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

        private void Load(object sender, RoutedEventArgs e)
        {
            _viewModel = new GenerateResourceWindowViewModel();
            _viewModel.Version = "v1";
            DataContext = _viewModel;
            RefreshAvailableActions();
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            RefreshAvailableActions();
        }
        
        private void Protect(object sender, RoutedEventArgs e)
        {
            foreach (var act in _viewModel.Actions)
            {
                string s = "";
            }
        }

        #endregion

        #region Private methods

        private Task RefreshAvailableActions()
        {
            _viewModel.IsLoading = true;
            _viewModel.Actions.Clear();
            return Task.Run(() =>
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
                            var functionName = function.Name;
                            var attributes = VisualStudioHelper.GetAllCodeElementsOfType(function.Attributes, vsCMElement.vsCMElementAttribute, false);
                            foreach (CodeAttribute attribute in attributes)
                            {
                                if (_httpAttributeFullNames.Contains(attribute.FullName))
                                {
                                    ExecuteRequestToUi(() =>
                                    {
                                        _viewModel.Actions.Add(new ControllerActionViewModel
                                        {
                                            Application = project.Name,
                                            Controller = codeClass.Name,
                                            Action = function.Name,
                                            IsSelected = true,
                                            DisplayName = $"{codeClass.Name}/{function.Name}"
                                        });
                                    });
                                }
                            }
                        }
                    }
                }

                ExecuteRequestToUi(() => _viewModel.IsLoading = false);
            });
        }

        private void ExecuteRequestToUi(Action callback)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                callback();
            }));
        }

        #endregion
    }
}