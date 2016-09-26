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
using System.ComponentModel;

namespace SimpleIdentityServer.Vse
{
    public class OptionsPageGrid : DialogPage
    {
        #region Private fields

        private string _url = "https://localhost:5444/api/vs/resources";
        private string _wellKnownConfigurationEdp = "https://localhost:5443/.well-known/openid-configuration";
        private string _clientId = "VisualStudioExtension";
        private string _clientSecret = "VisualStudioExtension";

        #endregion

        #region Public properties

        [Category("Configuration")]
        [DisplayName("URL")]
        [Description("URL used to create resources")]
        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        [Category("Configuration")]
        [DisplayName("Well known configuration")]
        [Description("Well known configuration endpoint")]
        public string WellKnownConfigurationEdp
        {
            get { return _wellKnownConfigurationEdp; }
            set { _wellKnownConfigurationEdp = value; }
        }

        [Category("Configuration")]
        [DisplayName("Client identifier")]
        [Description("Client identifier")]
        public string ClientId
        {
            get { return _clientId; }
            set { _clientId = value; }
        }

        [Category("Configuration")]
        [DisplayName("Client secret")]
        [Description("Client secret")]
        public string ClientSecret
        {
            get { return _clientSecret; }
            set { _clientSecret = value; }
        }

        #endregion
    }
}
