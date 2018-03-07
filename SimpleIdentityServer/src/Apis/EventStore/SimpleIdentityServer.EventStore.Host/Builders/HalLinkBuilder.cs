#region copyright
// Copyright 2017 Habart Thierry
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

using Newtonsoft.Json.Linq;
using System;

namespace SimpleIdentityServer.EventStore.Host.Builders
{
    public interface IHalLinkBuilder
    {
        IHalLinkBuilder AddSelfLink(string selfLink);
        JObject Build();
    }

    internal class HalLinkBuilder : IHalLinkBuilder
    {
        private JObject _obj;

        public IHalLinkBuilder AddSelfLink(string selfLink)
        {
            if (string.IsNullOrWhiteSpace(selfLink))
            {
                throw new ArgumentNullException(nameof(selfLink));
            }

            if (_obj == null)
            {
                _obj = new JObject();
            }

            var hrefProperty = new JProperty(Constants.HalLinkPropertyResponseNames.Href, selfLink);
            var hrefObj = new JObject(hrefProperty);
            var selfProperty = new JProperty(Constants.HalLinkResponseNames.Self, hrefObj);
            _obj.Add(selfProperty);
            return this;
        }

        public JObject Build()
        {
            var result = _obj.ToString();
            var clone = (JObject)_obj.DeepClone();
            _obj = null;
            return clone;
        }
    }
}
