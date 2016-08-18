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

using SimpleIdentityServer.Manager.Core.Api.Configuration.Actions;
using SimpleIdentityServer.Manager.Core.Parameters;
using System.Collections.Generic;

namespace SimpleIdentityServer.Manager.Core.Api.Configuration
{
    public interface IConfigurationActions
    {
        bool DeleteConfiguration(string key);

        List<SimpleIdentityServer.Core.Models.Configuration> GetConfigurations();

        SimpleIdentityServer.Core.Models.Configuration GetConfiguration(string key);

        bool UpdateConfiguration(UpdateConfigurationParameter updateConfigurationParameter);
    }

    internal class ConfigurationActions : IConfigurationActions
    {
        #region Fields

        private readonly IDeleteConfigurationAction _deleteConfigurationAction;

        private readonly IGetAllConfigurationAction _getAllConfigurationAction;

        private readonly IGetConfigurationAction _getConfigurationAction;

        private readonly IUpdateConfigurationAction _updateConfigurationAction;

        #endregion

        #region Constructor

        public ConfigurationActions(
            IDeleteConfigurationAction deleteConfigurationAction,
            IGetAllConfigurationAction getAllConfigurationAction,
            IGetConfigurationAction getConfigurationAction,
            IUpdateConfigurationAction updateConfigurationAction)
        {
            _deleteConfigurationAction = deleteConfigurationAction;
            _getAllConfigurationAction = getAllConfigurationAction;
            _getConfigurationAction = getConfigurationAction;
            _updateConfigurationAction = updateConfigurationAction;
        }

        #endregion

        #region Public methods

        public bool DeleteConfiguration(string key)
        {
            return _deleteConfigurationAction.Execute(key);
        }

        public List<SimpleIdentityServer.Core.Models.Configuration> GetConfigurations()
        {
            return _getAllConfigurationAction.Execute();
        }

        public SimpleIdentityServer.Core.Models.Configuration GetConfiguration(string key)
        {
            return _getConfigurationAction.Execute(key);
        }

        public bool UpdateConfiguration(UpdateConfigurationParameter updateConfigurationParameter)
        {
            return _updateConfigurationAction.Execute(updateConfigurationParameter);
        }

        #endregion
    }
}
