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

using System.Linq;
using System.Collections.Generic;
using System;
using SimpleIdentityServer.Uma.Core.Extensions;

namespace SimpleIdentityServer.Uma.Core.Responses
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
                return this[ResourceSetIdName].ToString();
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
                return this[ResourceSetIdName] as List<string>;
            }
            set
            {
                this.SetValue(ResourceSetIdName, value);
            }
        }

        public int Expiration
        {
            get
            {
                return int.Parse(this[ExpirationName].ToString());
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
                return bool.Parse(this[ActiveName].ToString());
            }
            set
            {
                this.SetValue(ActiveName, value);
            }
        }

        public int Expiration
        {
            get
            {
                return int.Parse(this[ExpirationName].ToString());
            }
            set
            {
                this.SetValue(ExpirationName, value);
            }
        }
        
        public int IssuedAt
        {
            get
            {
                return int.Parse(this[IatName].ToString());
            }
            set
            {
                this.SetValue(IatName, value);
            }
        }
        
        public int Nbf
        {
            get
            {
                return int.Parse(this[NbfName].ToString());
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
                return this[PermissionsName] as List<PermissionResponse>;
            }
            set
            {
                this.SetValue(PermissionsName, value);
            }
        }

        #endregion
    }
}
