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
using NuGet.VisualStudio;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.Selectors;
using SimpleIdentityServer.UmaManager.Client;
using SimpleIdentityServer.UmaManager.Client.DTOs.Requests;
using SimpleIdentityServer.UmaManager.Client.DTOs.Responses;
using SimpleIdentityServer.UmaManager.Client.Resources;
using SimpleIdentityServer.Vse.Extensions;
using SimpleIdentityServer.Vse.Helpers;
using SimpleIdentityServer.Vse.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private readonly IClientAuthSelector _clientAuthSelector;

        private bool _isInitialized;

        private GenerateProxyWindowViewModel _viewModel;

        private Options _options;

        private Project _selectedProject;

        #region Constructor

        public GenerateProxyWindowControl()
        {
            _isInitialized = false;
            var factory = new IdentityServerUmaManagerClientFactory();
            var identityServerClientFactory = new IdentityServerClientFactory();
            _clientAuthSelector = identityServerClientFactory.CreateTokenClient();
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
            var uri = CheckUri();
            if (uri == null)
            {
                return;
            }

            if (!_isInitialized)
            {
                _viewModel = new GenerateProxyWindowViewModel();
                _viewModel.IsLoading = true;
                DataContext = _viewModel;
                DisplayAllResources(uri);
                _isInitialized = true;
            }
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
            var uri = CheckUri();
            if (uri == null)
            {
                return;
            }

            var resource = _viewModel.Resources.FirstOrDefault(r => r.IsSelected);
            if (resource == null)
            {
                return;
            }

            InstallNugetPackages(_selectedProject, "Microsoft.Extensions.DependencyInjection", "1.0.0")
                .ContinueWith((firstTask) =>
                {
                    if (!firstTask.Result)
                    {
                        return;
                    }

                    InstallNugetPackages(_selectedProject, "SimpleIdentityServer.Proxy")
                        .ContinueWith((secondTask) =>
                        {
                            if (!secondTask.Result)
                            {
                                return;
                            }
                            
                            AddSecurityProxy(_selectedProject, resource);
                            AddAuthProvider(_selectedProject);
                        });
                });
        }

        #endregion

        #region Private methods
        
        private void AddSecurityProxy(Project project, ResourceViewModel resource)
        {
            try
            {               
                // 1. Update the content
                var folder = project.GetRootFolder();
                string content = GetSecurityProxyContent();
                content = content.Replace("{hash}", resource.Hash);
                content = content.Replace("{resource_url}", resource.Name);
                content = content.Replace("{namespace}", project.Name);

                // 2. Add the security proxy to the project
                var fileName = string.Format("SecurityProxy_{0}.cs", resource.Hash);
                var path = Path.Combine(folder, fileName);
                if (File.Exists(path))
                {
                    MessageBox.Show($"The file {fileName} already exists");
                    return;
                }
                
                using (var writer = new StreamWriter(path))
                {
                    writer.Write(content);
                }

                project.AddFileToProject(path, _options.Dte2);
            }
            catch
            {
                MessageBox.Show("Error occured while trying to add the file");
            }
        }

        private void AddAuthProvider(Project project)
        {
            try
            {
                // 1. Update the content
                var folder = project.GetRootFolder();
                string content = GetAuthProviderContent();
                content = content.Replace("{namespace}", project.Name);

                // 2. Add the security proxy to the project
                var fileName = "AuthProvider.cs";
                var path = Path.Combine(folder, fileName);
                if (File.Exists(path))
                {
                    MessageBox.Show($"The file {fileName} already exists");
                    return;
                }

                using (var writer = new StreamWriter(path))
                {
                    writer.Write(content);
                }

                project.AddFileToProject(path, _options.Dte2);
            }
            catch
            {
                MessageBox.Show("Error occured while trying to add the file");
            }
        }
                
        private async Task SearchResources(string url, Uri uri)
        {
            var accessToken = await GetAccessToken();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                await DisplayResources(() =>
                {
                    return _resourceClient.SearchResources(new SearchResourceRequest
                    {
                        Url = url
                    }, uri, accessToken);
                });
            }
            else
            {
                MessageBox.Show("An error occured while trying to retrieve the access token");
            }
        }

        private async Task DisplayAllResources(Uri uri)
        {
            var accessToken = await GetAccessToken();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                await DisplayResources(() =>
                {
                    return _resourceClient.GetResources(uri, accessToken);
                });
            }
            else
            {
                MessageBox.Show("An error occured while trying to retrieve the access token");
            }
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
                            Name = resource.Url,
                            Hash = resource.Hash
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
        
        private string GetSecurityProxyContent()
        {
            return GetContent("SecurityProxy.cs.txt");
        }

        private string GetAuthProviderContent()
        {
            return GetContent("AuthProvider.cs.txt");
        }

        private string GetContent(string fileName)
        {
            string result = null;
            var assembly = Assembly.GetExecutingAssembly().Location;
            var codeFolder = Path.Combine(Path.GetDirectoryName(assembly), "Codes");
            var codeFile = Path.Combine(codeFolder, fileName);
            using (var reader = new StreamReader(codeFile))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        private Task<bool> InstallNugetPackages(
            Project project,
            string package,
            string version = null)
        {
            var vsPackageInstallerServices = _options.ComponentModel.GetService<IVsPackageInstallerServices>();
            var vsPackageInstaller = _options.ComponentModel.GetService<IVsPackageInstaller>();
            return Task.Run(() =>
            {
                try
                {
                    if (!vsPackageInstallerServices.IsPackageInstalled(project, package))
                    {
                        _options.Dte.StatusBar.Text = $"Install {package} Nuget package, this may takes a minute ...";
                        vsPackageInstaller.InstallPackage(null, project, package, version, false);
                        _options.Dte.StatusBar.Text = $"Finished installing the {package} Nuget package";
                    }
                }
                catch
                {
                    ExecuteRequestToUi(() => MessageBox.Show($"Error occured while trying to add the {package} Nuget package"));
                    return false;
                }

                return true;
            });
        }

        private async Task<string> GetAccessToken()
        {
            var url = _options.Package.WellKnownConfigurationEdp;
            try
            {
                var response = await _clientAuthSelector.UseClientSecretPostAuth(Constants.ClientId, Constants.ClientSecret)
                    .UseClientCredentials(Constants.Scope)
                    .ResolveAsync(url);
                if (response == null)
                {
                    return null;
                }

                return response.AccessToken;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}