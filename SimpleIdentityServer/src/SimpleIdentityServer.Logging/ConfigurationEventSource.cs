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

using Microsoft.Extensions.Logging;
using System;

namespace SimpleIdentityServer.Logging
{
    public interface IConfigurationEventSource
    {
        #region Events linked to authentication providers

        void EnableAuthenticationProvider(string providerName, bool enabled);

        void FinishToEnableAuthenticationProvider(string providerName, bool enabled);

        void StartToAddAuthenticationProvider(string request);

        void FinishToAddAuthenticationProvider(string result);

        void StartToRemoveAuthenticationProvider(string name);

        void FinishToRemoveAuthenticationProvider(string name);

        #endregion

        #region Events linked to settings

        void StartToDropSetting(string name);

        void FinishToDropSetting(string name);

        #endregion

        void Failure(string message);

        void Failure(Exception exception);
    }

    public class ConfigurationEventSource : IConfigurationEventSource
    {
        private static class Tasks
        {
            public const string AuthenticationProvider = "AuthenticationProvider";
            public const string Setting = "Setting";
            public const string Failure = "Failure";
        }

        private const string MessagePattern = "{Id} : {Task}, {Message} : {Operation}";

        #region Fields

        private readonly ILogger _logger;

        #endregion

        #region Constructor

        public ConfigurationEventSource(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ConfigurationEventSource>();
        }

        #endregion

        #region Events linked to authentication providers

        public void EnableAuthenticationProvider(string providerName, bool enabled)
        {
            string enable = GetEnableStr(enabled);
            var evt = new Event
            {
                Id = 1,
                Task = Tasks.AuthenticationProvider,
                Message = $"{enable} authentication provider {providerName}"
            };

            LogInformation(evt);
        }

        public void FinishToEnableAuthenticationProvider(string providerName, bool enabled)
        {
            string enable = GetEnableStr(enabled);
            var evt = new Event
            {
                Id = 2,
                Task = Tasks.AuthenticationProvider,
                Message = $"Finish to {enable} authentication provider {providerName}"
            };

            LogInformation(evt);
        }

        public void StartToAddAuthenticationProvider(string request)
        {
            var evt = new Event
            {
                Id = 3,
                Task = Tasks.AuthenticationProvider,
                Message = $"Start to add authentication provider {request}"
            };

            LogInformation(evt);
        }

        public void FinishToAddAuthenticationProvider(string result)
        {
            var evt = new Event
            {
                Id = 4,
                Task = Tasks.AuthenticationProvider,
                Message = $"Finish to add authentication provider {result}"
            };

            LogInformation(evt);
        }

        public void StartToRemoveAuthenticationProvider(string name)
        {
            var evt = new Event
            {
                Id = 5,
                Task = Tasks.AuthenticationProvider,
                Message = $"Start to remove authentication provider {name}"
            };

            LogInformation(evt);
        }

        public void FinishToRemoveAuthenticationProvider(string name)
        {
            var evt = new Event
            {
                Id = 6,
                Task = Tasks.AuthenticationProvider,
                Message = $"Finish to remove authentication provider {name}"
            };

            LogInformation(evt);
        }

        #endregion

        #region Events linkeds to settings

        public void StartToDropSetting(string name)
        {
            var evt = new Event
            {
                Id = 7,
                Task = Tasks.Setting,
                Message = $"Start to remove setting {name}"
            };

            LogInformation(evt);
        }

        public void FinishToDropSetting(string name)
        {
            var evt = new Event
            {
                Id = 8,
                Task = Tasks.Setting,
                Message = $"Finish to remove setting {name}"
            };

            LogInformation(evt);
        }

        #endregion

        public void Failure(string message)
        {
            var evt = new Event
            {
                Id = 998,
                Task = Tasks.Failure,
                Message = $"Something goes wrong, code : {message}"
            };

            LogError(evt);
        }

        public void Failure(Exception exception)
        {
            var evt = new Event
            {
                Id = 999,
                Task = Tasks.Failure,
                Message = "an error occured"
            };

            LogError(evt, new EventId(999), exception);
        }

        #region Private methods

        private void LogInformation(Event evt)
        {
            _logger.LogInformation(MessagePattern, evt.Id, evt.Task, evt.Message, evt.Operation);
        }

        private void LogError(Event evt)
        {
            _logger.LogError(MessagePattern, evt.Id, evt.Task, evt.Message, evt.Operation);
        }

        private void LogError(Event evt, EventId evtId, Exception ex)
        {
            _logger.LogError(evtId, ex, MessagePattern, evt.Id, evt.Task, evt.Message, evt.Operation);
        }

        private static string GetEnableStr(bool isEnabled)
        {
            return isEnabled ? "Enable" : "Disable";
        }

        #endregion
    }
}
