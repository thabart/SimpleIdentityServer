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
using SimpleIdentityServer.UmaManager.Client.DTOs.Responses;
using SimpleIdentityServer.UmaManager.Client.Resources;
using SimpleIdentityServer.Vse.Helpers;
using SimpleIdentityServer.Vse.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SimpleIdentityServer.Vse
{
    public partial class GenerateProxyWindowControl : UserControl
    {
        private readonly IResourceClient _resourceClient;

        private GenerateProxyWindowViewModel _viewModel;

        private Options _options;

        #region Constructor

        public GenerateProxyWindowControl()
        {
            var factory = new IdentityServerUmaManagerClientFactory();
            _resourceClient = factory.GetResourceClient();
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

            _options = options;
        }

        #endregion

        #region Event handlers

        private void Load(object sender, RoutedEventArgs e)
        {
            var uri = CheckUri();
            if (uri == null)
            {
                return;
            }

            _viewModel = new GenerateProxyWindowViewModel();
            _viewModel.IsLoading = true;
            DataContext = _viewModel;
            DisplayAllResources(uri);
        }
        
        private void Search(object sender, RoutedEventArgs e)
        {
            var uri = CheckUri();
            if (uri == null)
            {
                return;
            }
            
            _viewModel.IsLoading = true;
            _viewModel.Resources.Clear();
            SearchResources(_viewModel.Query, uri);
        }
        
        private void GenerateProxy(object sender, RoutedEventArgs e)
        {
            var project = GetSelectedProject();
            if (project == null)
            {
                return;
            }

            var folder = project.GetRootFolder();
            string result = GetSecurityProxyContent();
            var id = Guid.NewGuid().ToString();
            result = result.Replace("{guid}", id);
            var fileName = string.Format("SecurityProxy_{0}.cs", id);
            var path = Path.Combine(folder, fileName);
            if (File.Exists(path))
            {
                MessageBox.Show("The file '" + fileName + "' already exists");
                return;
            }

            using (var writer = new StreamWriter(path))
            {
                writer.Write(result);
            }

            project.AddFileToProject(path, _options.Dte);
        }

        #endregion

        #region Private methods

        private async Task SearchResources(string query, Uri uri)
        {
            await DisplayResources(() =>
            {
                return _resourceClient.SearchResources(query, uri, string.Empty);
            });
        }

        private async Task DisplayAllResources(Uri uri)
        {
            await DisplayResources(() =>
            {
                return _resourceClient.GetResources(uri, string.Empty);
            });
        }

        private async Task DisplayResources(Func<Task<List<ResourceResponse>>> callback)
        {
            try
            {
                var resources = await callback();
                ExecuteRequestToUi(() =>
                {
                    foreach (var resource in resources)
                    {
                        _viewModel.Resources.Add(new ResourceViewModel
                        {
                            IsSelected = false,
                            Name = resource.Url
                        });
                    }

                    _viewModel.IsLoading = false;
                });
            }
            catch
            {
                ExecuteRequestToUi(() =>
                {
                    MessageBox.Show("Resources cannot be retrieved");
                    _viewModel.IsLoading = false;
                });
            }
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

        private void ExecuteRequestToUi(Action callback)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                callback();
            }));
        }

        private Project GetSelectedProject()
        {
            var items = (Array)_options.Dte.ToolWindows.SolutionExplorer.SelectedItems;
            foreach (UIHierarchyItem selItem in items)
            {
                return selItem.Object as Project;
            }

            return null;
        }

        private string GetSecurityProxyContent()
        {
            string result = null;
            var assembly = Assembly.GetExecutingAssembly().Location;
            var codeFolder = Path.Combine(Path.GetDirectoryName(assembly), "Codes");
            var codeFile = Path.Combine(codeFolder, "SecurityProxy.cs.txt");
            using (var reader = new StreamReader(codeFile))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        private void InstallNugetPackages()
        {
        }

        #endregion
    }
}