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

using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Scim.Client.Factories;
using SimpleIdentityServer.Scim.Client.Groups;
using System;

namespace SimpleIdentityServer.Scim.Client
{
    public interface IScimClientFactory
    {
        IGroupsClient GetGroupClient();
    }

    internal class ScimClientFactory : IScimClientFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ScimClientFactory()
        {
            var services = new ServiceCollection();
            RegisterDependencies(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        public IGroupsClient GetGroupClient()
        {
            var groupsClient = (IGroupsClient)_serviceProvider.GetService(typeof(IGroupsClient));
            return groupsClient;
        }

        private static void RegisterDependencies(IServiceCollection services)
        {
            services.AddTransient<IGroupsClient, GroupsClient>();
            services.AddTransient<IHttpClientFactory, HttpClientFactory>();
        }
    }
}
