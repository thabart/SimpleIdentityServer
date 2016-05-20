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

using SimpleIdentityServer.Client.Extensions;
using System.Collections.Generic;

namespace SimpleIdentityServer.Client.DTOs.Responses
{

    public class PermissionResponse : Dictionary<string, object>
    {
        #region Fields

        private const string ResourceSetIdName = "resource_set_id";

        private const string ScopesName = "scopes";

        private const string ExpirationName = "exp";

        #endregion

        #region Properties

        public string ResourceSetId
        {
            get
            {
                return this.GetString(ResourceSetIdName);
            }
            set
            {
                this.SetValue(ResourceSetIdName, value);
            }
        }

        public List<string> Scopes
        {
            get
            {
                return this.GetObject<List<string>>(ScopesName);
            }
            set
            {
                this.SetObject(ScopesName, value);
            }
        }

        public double Expiration
        {
            get
            {
                return this.GetDouble(ExpirationName);
            }
            set
            {
                this.SetValue(ExpirationName, value);
            }
        }

        #endregion
    }

    public class IntrospectionResponse : Dictionary<string, object>
    {
        #region Fields

        private const string ActiveName = "active";

        private const string ExpirationName = "exp";

        private const string IatName = "iat";

        private const string NbfName = "nbf";

        private const string PermissionsName = "permissions";

        #endregion

        #region Properties

        public bool IsActive
        {
            get
            {
                return this.GetBoolean(ActiveName);
            }
            set
            {
                this.SetValue(ActiveName, value);
            }
        }

        public double Expiration
        {
            get
            {
                return this.GetDouble(ExpirationName);
            }
            set
            {
                this.SetValue(ExpirationName, value);
            }
        }

        public double IssuedAt
        {
            get
            {
                return this.GetDouble(IatName);
            }
            set
            {
                this.SetValue(IatName, value);
            }
        }

        public double Nbf
        {
            get
            {
                return this.GetDouble(NbfName);
            }
            set
            {
                this.SetValue(NbfName, value);
            }
        }

        public List<PermissionResponse> Permissions
        {
            get
            {
                return this.GetObject<List<PermissionResponse>>(PermissionsName);
            }
            set
            {
                this.SetObject(PermissionsName, value);
            }
        }

        #endregion
    }
}
