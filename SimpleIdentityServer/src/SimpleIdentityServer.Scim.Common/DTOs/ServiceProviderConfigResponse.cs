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

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Scim.Common.DTOs
{
    [DataContract]
    public sealed class BulkResponse
    {
        [DataMember(Name = Constants.BulkResponseNames.Supported)]
        public bool Supported { get; set; }

        [DataMember(Name = Constants.BulkResponseNames.MaxOperations)]
        public int MaxOperations { get; set; }

        [DataMember(Name = Constants.BulkResponseNames.MaxPayloadSize)]
        public int MaxPayloadSize { get; set; }
    }

    [DataContract]
    public sealed class PatchResponse
    {
        [DataMember(Name = Constants.PatchResponseNames.Supported)]
        public bool Supported { get; set; }
    }

    [DataContract]
    public sealed class FilterResponse
    {
        [DataMember(Name = Constants.FilterResponseNames.Supported)]
        public bool Supported { get; set; }

        [DataMember(Name = Constants.FilterResponseNames.MaxResults)]
        public int MaxResults { get; set; }
    }

    [DataContract]
    public sealed class ChangePasswordResponse
    {
        [DataMember(Name = Constants.ChangePasswordResponseNames.Supported)]
        public bool Supported { get; set; }
    }

    [DataContract]
    public sealed class SortResponse
    {
        [DataMember(Name = Constants.SortResponseNames.Supported)]
        public bool Supported { get; set; }
    }

    [DataContract]
    public sealed class EtagResponse
    {
        [DataMember(Name = Constants.EtagResponseNames.Supported)]
        public bool Supported { get; set; }
    }

    [DataContract]
    public sealed class AuthenticationSchemeResponse : MultiValueAttr
    {
        [DataMember(Name = Constants.AuthenticationSchemeResponseNames.Name)]
        public string Name { get; set; }

        [DataMember(Name = Constants.AuthenticationSchemeResponseNames.Description)]
        public string Description { get; set; }

        [DataMember(Name = Constants.AuthenticationSchemeResponseNames.SpecUri)]
        public string SpecUri { get; set; }

        [DataMember(Name = Constants.AuthenticationSchemeResponseNames.DocumentationUri)]
        public string DocumentationUri { get; set; }
    }

    [DataContract]
    public sealed class ServiceProviderConfigResponse : ScimResource
    {
        [DataMember(Name = Constants.ServiceProviderConfigResponseNames.DocumentationUri)]
        public string DocumentationUri { get; set; }

        [DataMember(Name = Constants.ServiceProviderConfigResponseNames.Patch)]
        public PatchResponse Patch { get; set; }

        [DataMember(Name = Constants.ServiceProviderConfigResponseNames.Bulk)]
        public BulkResponse Bulk { get; set; }

        [DataMember(Name = Constants.ServiceProviderConfigResponseNames.Filter)]
        public FilterResponse Filter { get; set; }

        [DataMember(Name = Constants.ServiceProviderConfigResponseNames.ChangePassword)]
        public ChangePasswordResponse ChangePassword { get; set; }

        [DataMember(Name = Constants.ServiceProviderConfigResponseNames.Sort)]
        public SortResponse Sort { get; set; }

        [DataMember(Name = Constants.ServiceProviderConfigResponseNames.Etag)]
        public EtagResponse Etag { get; set; }

        [DataMember(Name = Constants.ServiceProviderConfigResponseNames.AuthenticationSchemes)]
        public IEnumerable<AuthenticationSchemeResponse> AuthenticationSchemes { get; set; }
    }
}
