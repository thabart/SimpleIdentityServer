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

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace SimpleIdentityServer.RateLimitation
{
    public class SimpleTypeFilterAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        #region Fields

        private ObjectFactory factory;

        #endregion

        #region Constructor

        public SimpleTypeFilterAttribute(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            ServiceType = serviceType;
        }

        #endregion

        #region Properties

        public int Order { get; set; }

        public Type ServiceType { get; set; }
        
        public object[] Arguments { get; set; }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Public methods

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            if (this.factory == null)
            {
                var argumentTypes = Arguments?.Select(a => a.GetType())?.ToArray();
                this.factory = ActivatorUtilities.CreateFactory(ServiceType, argumentTypes ?? Type.EmptyTypes);
            }

            return (IFilterMetadata)this.factory(serviceProvider, Arguments);
        }

        #endregion
    }
}
