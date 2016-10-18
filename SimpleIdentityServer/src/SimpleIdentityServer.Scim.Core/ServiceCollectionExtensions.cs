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
using SimpleIdentityServer.Scim.Core.Apis;
using SimpleIdentityServer.Scim.Core.Factories;
using SimpleIdentityServer.Scim.Core.Parsers;
using SimpleIdentityServer.Scim.Core.Stores;
using SimpleIdentityServer.Scim.Core.Validators;
using System;

namespace SimpleIdentityServer.Scim.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScim(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddTransient<ISchemaStore, SchemaStore>();
            services.AddTransient<IRequestParser, RequestParser>();
            services.AddTransient<IResponseParser, ResponseParser>();
            services.AddTransient<IAddRepresentationAction, AddRepresentationAction>();
            services.AddTransient<IGetRepresentationAction, GetRepresentationAction>();
            services.AddTransient<IDeleteRepresentationAction, DeleteRepresentationAction>();
            services.AddTransient<IUpdateRepresentationAction, UpdateRepresentationAction>();
            services.AddTransient<IGroupsAction, GroupsAction>();
            services.AddTransient<IApiResponseFactory, ApiResponseFactory>();
            services.AddTransient<IParametersValidator, ParametersValidator>();
            return services;
        }
    }
}
