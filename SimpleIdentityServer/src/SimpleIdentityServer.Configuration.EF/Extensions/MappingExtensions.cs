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
using System.Linq;
using Domain = SimpleIdentityServer.Configuration.Core.Models;
using Model = SimpleIdentityServer.Configuration.EF.Models;

namespace SimpleIdentityServer.Configuration.EF.Extensions
{
    internal static class MappingExtensions
    {
        #region To Domain object

        public static Domain.Option ToDomain(this Model.Option option)
        {
            return new Domain.Option
            {
                Id = option.Id,
                Key = option.Key,
                Value = option.Value
            };
        }

        public static Domain.AuthenticationProvider ToDomain(this Model.AuthenticationProvider authenticationProvider)
        {
            var options = new List<Domain.Option>();
            if (authenticationProvider.Options != null)
            {
                options = authenticationProvider.Options.Select(a => a.ToDomain()).ToList();
            }

            return new Domain.AuthenticationProvider
            {
                IsEnabled = authenticationProvider.IsEnabled,
                Name = authenticationProvider.Name,
                Options = options
            };
        }

        public static Domain.Setting ToDomain(this Model.Setting setting)
        {
            return new Domain.Setting
            {
                Key = setting.Key,
                Value = setting.Value
            };
        }

        #endregion

        #region To model

        public static Model.Option ToModel(this Domain.Option option)
        {
            return new Model.Option
            {
                Id = option.Id,
                Key = option.Key,
                Value = option.Value
            };
        }

        public static Model.AuthenticationProvider ToModel(this Domain.AuthenticationProvider authenticationProvider)
        {
            var options = new List<Model.Option>();
            if (authenticationProvider.Options != null)
            {
                options = authenticationProvider.Options.Select(a => a.ToModel()).ToList();
            }

            return new Model.AuthenticationProvider
            {
                IsEnabled = authenticationProvider.IsEnabled,
                Name = authenticationProvider.Name,
                Options = options
            };
        }

        public static Model.Setting ToModel(this Domain.Setting setting)
        {
            return new Model.Setting
            {
                Key = setting.Key,
                Value = setting.Value
            };
        }

        #endregion
    }
}
