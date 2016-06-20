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
using SimpleIdentityServer.Vse.Helpers;
using SimpleIdentityServer.Vse.ViewModels;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace SimpleIdentityServer.Vse
{
    public partial class GenerateProxyWindowControl : UserControl
    {
        private GenerateProxyWindowViewModel _viewModel;

        private Options _options;

        #region Constructor

        public GenerateProxyWindowControl()
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

            _options = options;
        }

        #endregion

        #region Private methods

        private void Load(object sender, RoutedEventArgs e)
        {
            _viewModel = new GenerateProxyWindowViewModel();
            _viewModel.Resources.Add(new ResourceViewModel
            {
                Name = "name",
                IsSelected = false
            });
            DataContext = _viewModel;
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
                
        #endregion
    }
}