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

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SimpleIdentityServer.Vse.ViewModels
{
    public class GenerateProxyWindowViewModel : INotifyPropertyChanged
    {
        #region Fields

        private bool _isLoading;

        #endregion

        #region Properties

        public ObservableCollection<ResourceViewModel> Resources { get; set; }

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                _isLoading = value;
                NotifyChange("IsLoading");
            }
        }

        public string Query { get; set; }

        #endregion

        #region Constructor

        public GenerateProxyWindowViewModel()
        {
            _isLoading = true;
            Resources = new ObservableCollection<ResourceViewModel>();
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Private methods

        private void NotifyChange(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
