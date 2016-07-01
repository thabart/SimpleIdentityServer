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
using SimpleIdentityServer.UmaManager.Client;
using SimpleIdentityServer.UmaManager.Client.DTOs.Requests;
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

            // 1. Add the nuget packages
            InstallNugetPackages(_selectedProject, "Microsoft.Extensions.DependencyInjection", "1.0.0-rc2-final")
                .ContinueWith((firstTask) =>
                {
                    if (!firstTask.Result)
                    {
                        return;
                    }

                    InstallNugetPackages(_selectedProject, "SimpleIdentityServer.UmaIntrospection.Authentication");
                    InstallNugetPackages(_selectedProject, "SimpleIdentityServer.Uma.Authorization");
                });

            // 2. Protect the actions
            ProtectActions(uri);

            // 3. Add file
            AddStartup(_selectedProject, uri);
        }

        #endregion

        #region Private methods

        private async Task ProtectActions(Uri uri)
        {
            ExecuteRequestToUi(() => _viewModel.IsLoading = true);
            foreach (var act in _viewModel.Actions.Where(a => a.IsSelected))
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
            if (_selectedProject == null)
            {
                return Task.FromResult(0);
            }

            _viewModel.IsLoading = true;
            _viewModel.Actions.Clear();
            var selectedProjectName = _selectedProject.Name;
            return Task.Run(() =>
            {
                var classes = _selectedProject.GetClasses();
                foreach (CodeClass codeClass in classes)
                {
                    if (codeClass.IsDerivedFrom["Microsoft.AspNetCore.Mvc.Controller"])
                    {
                        var className = codeClass.Name;
                        var functions = VisualStudioHelper.GetAllCodeElementsOfType(codeClass.Members, vsCMElement.vsCMElementFunction, false);
                        foreach (CodeFunction function in functions)
                        {
                            var attributes = VisualStudioHelper.GetAllCodeElementsOfType(function.Attributes, vsCMElement.vsCMElementAttribute, false);
                            var parameters = VisualStudioHelper.GetAllCodeElementsOfType(function.Parameters, vsCMElement.vsCMElementParameter, false);
                            var parameterNames = new List<string>();
                            foreach (CodeParameter parameter in parameters)
                            {
                                parameterNames.Add(parameter.Name);
                            }

                            var functionName = function.Name;
                            if (parameterNames.Any())
                            {
                                functionName = functionName+"{"+string.Join(",", parameterNames) +"}";
                            }

                            foreach (CodeAttribute attribute in attributes)
                            {
                                if (_httpAttributeFullNames.Contains(attribute.FullName))
                                {
                                    ExecuteRequestToUi(() =>
                                    {
                                        _viewModel.Actions.Add(new ControllerActionViewModel
                                        {
                                            Application = selectedProjectName,
                                            Controller = className,
                                            Action = functionName,
                                            IsSelected = true,
                                            DisplayName = $"{className}/{functionName}"
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

        private void AddStartup(Project project, Uri uri)
        {
            try
            {
                // 1. Update the content
                var folder = project.GetRootFolder();
                string content = GetStartup();
                content = content.Replace("{namespace}", project.Name);
                content = content.Replace("{resource_url}", uri.AbsoluteUri);

                // 2. Add the security proxy to the project
                var fileName = "Startup_Sample.cs";
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

        private string GetStartup()
        {
            return GetContent("Startup_Sample.cs.txt");
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

        #endregion
    }
}