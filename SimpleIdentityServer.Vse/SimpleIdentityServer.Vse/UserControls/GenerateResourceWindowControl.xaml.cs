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
using SimpleIdentityServer.UmaManager.Client;
using SimpleIdentityServer.UmaManager.Client.DTOs.Requests;
using SimpleIdentityServer.UmaManager.Client.Resources;
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
        #region Fields

        private readonly IResourceClient _resourceClient;

        private Options _options;

        private bool _isInitialized;

        private List<string> _httpAttributeFullNames = new List<string>
        {
            "Microsoft.AspNetCore.Mvc.HttpGetAttribute",
            "Microsoft.AspNetCore.Mvc.HttpPutAttribute",
            "Microsoft.AspNetCore.Mvc.HttpPostAttribute",
            "Microsoft.AspNetCore.Mvc.HttpDeleteAttribute"
        };

        private GenerateResourceWindowViewModel _viewModel;

        private Project _selectedProject;

        #endregion

        #region Constructor

        public GenerateResourceWindowControl()
        {
            _isInitialized = false;
            InitializeComponent();
            var factory = new IdentityServerUmaManagerClientFactory();
            _resourceClient = factory.GetResourceClient();
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

            _selectedProject = options.Dte2.GetSelectedProject();
            if (_selectedProject == null)
            {
                throw new InvalidOperationException("Select a project before opening the panel");
            }

            _options = options;
        }

        #endregion

        #region Event handlers

        private void Load(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
            {
                _viewModel = new GenerateResourceWindowViewModel();
                _viewModel.Version = "v1";
                DataContext = _viewModel;
                RefreshAvailableActions();
                _isInitialized = true;
            }
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            RefreshAvailableActions();
        }
        
        private void Protect(object sender, RoutedEventArgs e)
        {
            var uri = CheckUri();
            if (uri == null)
            {
                return;
            }

            ProtectActions(uri);
        }

        #endregion

        #region Private methods

        private async Task ProtectActions(Uri uri)
        {
            ExecuteRequestToUi(() => _viewModel.IsLoading = true);
            foreach (var act in _viewModel.Actions)
            {
                await _resourceClient.AddControllerAction(new AddControllerActionRequest
                {
                    Action = act.Action,
                    Application = act.Application,
                    Controller = act.Controller,
                    Version = _viewModel.Version
                }, uri, string.Empty);
            }

            ExecuteRequestToUi(() => _viewModel.IsLoading = false);
        }

        private Task RefreshAvailableActions()
        {
            _viewModel.IsLoading = true;
            _viewModel.Actions.Clear();
            return Task.Run(() =>
            {
                var classes = _selectedProject.GetClasses();
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
                                            Application = _selectedProject.Name,
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

        private Uri CheckUri()
        {
            var url = _options.Package.Url;
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("url cannot be empty");
                return null;
            }

            Uri uri = null;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                MessageBox.Show($"{url} is not a well formatted url");
                return null;
            }

            return uri;
        }

        #endregion
    }
}