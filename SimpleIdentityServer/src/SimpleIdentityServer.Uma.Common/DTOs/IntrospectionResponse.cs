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

using SimpleIdentityServer.Uma.Common.Extensions;
using System.Collections.Generic;

namespace SimpleIdentityServer.Uma.Common.DTOs
{

    public class PermissionResponse : Dictionary<string, object>
    {
        public string ResourceSetId
        {
            get
            {
                return this.GetString(IntrospectPermissionNames.ResourceSetIdName);
            }
            set
            {
                this.SetValue(IntrospectPermissionNames.ResourceSetIdName, value);
            }
        }

        public List<string> Scopes
        {
            get
            {
                return this.GetObject<List<string>>(IntrospectPermissionNames.ScopesName);
            }
            set
            {
                this.SetObject(IntrospectPermissionNames.ScopesName, value);
            }
        }

        public double Expiration
        {
            get
            {
                return this.GetDouble(IntrospectPermissionNames.ExpirationName);
            }
            set
            {
                this.SetValue(IntrospectPermissionNames.ExpirationName, value);
            }
        }
    }

    public class IntrospectionResponse : Dictionary<string, object>
    {

        public bool IsActive
        {
            get
            {
                return this.GetBoolean(IntrospectNames.ActiveName);
            }
            set
            {
                this.SetValue(IntrospectNames.ActiveName, value);
            }
        }

        public double Expiration
        {
            get
            {
                return this.GetDouble(IntrospectNames.ExpirationName);
            }
            set
            {
                this.SetValue(IntrospectNames.ExpirationName, value);
            }
        }

        public double IssuedAt
        {
            get
            {
                return this.GetDouble(IntrospectNames.IatName);
            }
            set
            {
                this.SetValue(IntrospectNames.IatName, value);
            }
        }

        public double Nbf
        {
            get
            {
                return this.GetDouble(IntrospectNames.NbfName);
            }
            set
            {
                this.SetValue(IntrospectNames.NbfName, value);
            }
        }

        public List<PermissionResponse> Permissions
        {
            get
            {
                return this.GetObject<List<PermissionResponse>>(IntrospectNames.PermissionsName);
            }
            set
            {
                this.SetObject(IntrospectNames.PermissionsName, value);
            }
        }
    }
}
