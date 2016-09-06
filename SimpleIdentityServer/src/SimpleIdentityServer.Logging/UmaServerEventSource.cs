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
using SimpleIdentityServer.Logging;
using System;

namespace SimpleIdentityServer.Uma.Logging
{
    public interface IUmaServerEventSource
    {
        #region Events linked to the authorization

        void StartGettingAuthorization(string request);

        void CheckAuthorizationPolicy(string request);

        void RequestIsNotAuthorized(string request);

        void RequestIsAuthorized(string request);

        void AuthorizationPolicyFailed(string policyId);

        #endregion

        #region Events linked to introspection

        void StartToIntrospect(string rpt);

        void RptHasExpired(string rpt);

        void EndIntrospection(string result);

        #endregion

        #region Events linked to permission

        void StartAddPermission(string request);

        void FinishAddPermission(string request);

        #endregion

        #region Events linked to authorization policy

        void StartAddingAuthorizationPolicy(string request);

        void FinishToAddAuthorizationPolicy(string result);

        void StartToRemoveAuthorizationPolicy(string policyId);

        void FinishToRemoveAuthorizationPolicy(string policyId);

        #endregion

        #region Events linked to resource set

        void StartToAddResourceSet(string request);

        void FinishToAddResourceSet(string result);

        void StartToRemoveResourceSet(string resourceSetId);

        void FinishToRemoveResourceSet(string resourceSetId);

        #endregion

        #region Events linked to scope

        void StartToAddScope(string request);

        void FinishToAddScope(string result);

        void StartToRemoveScope(string scope);

        void FinishToRemoveScope(string scope);

        #endregion

        void Failure(string message);

        void Failure(Exception exception);
    }

    public class UmaServerEventSource : IUmaServerEventSource
    {
        private readonly ILogger _logger;

        private const string MessagePattern = "{Id} : {Task}, {Message} : {Operation}";

        private static class Tasks
        {
            public const string Authorization = "Authorization";
            public const string Introspection = "Introspection";
            public const string Permission = "Permission";
            public const string AuthorizationPolicy = "AuthorizationPolicy";
            public const string ResourceSet = "ResourceSet";
            public const string Scope = "Scope";
            public const string Failure = "Failure";
        }

        #region Constructor

        public UmaServerEventSource(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UmaServerEventSource>();
        }

        #endregion

        #region Events linked to the authorization

        public void StartGettingAuthorization(string request)
        {
            var evt = new Event
            {
                Id = 1,
                Task = Tasks.Authorization,
                Message = $"Start getting RPT token : {request}"
            };

            LogInformation(evt);
        }

        public void CheckAuthorizationPolicy(string request)
        {
            var evt = new Event
            {
                Id = 2,
                Task = Tasks.Authorization,
                Message = $"Check authorization policy : {request}"
            };

            LogInformation(evt);
        }

        public void AuthorizationPolicyFailed(string policyId)
        {
            var evt = new Event
            {
                Id = 3,
                Task = Tasks.Authorization,
                Message = $"Authorization policy {policyId} failed"
            };

            LogInformation(evt);
        }

        public void RequestIsNotAuthorized(string request)
        {
            var evt = new Event
            {
                Id = 4,
                Task = Tasks.Authorization,
                Message = $"Request is not authorized : {request}",
                Operation = "not-authorized"
            };

            LogInformation(evt);
        }

        public void RequestIsAuthorized(string request)
        {
            var evt = new Event
            {
                Id = 5,
                Task = Tasks.Authorization,
                Message = $"Request is authorized : {request}",
                Operation = "authorized"
            };

            LogInformation(evt);
        }

        #endregion

        #region Events linked to introspection

        public void StartToIntrospect(string rpt)
        {
            var evt = new Event
            {
                Id = 6,
                Task = Tasks.Introspection,
                Message = $"Start to introspect the RPT {rpt}"
            };

            LogInformation(evt);
        }

        public void RptHasExpired(string rpt)
        {
            var evt = new Event
            {
                Id = 7,
                Task = Tasks.Introspection,
                Message = $"RPT {rpt} has expired"
            };

            LogInformation(evt);
        }

        public void EndIntrospection(string result)
        {
            var evt = new Event
            {
                Id = 8,
                Task = Tasks.Introspection,
                Message = $"End introspection {result}"
            };

            LogInformation(evt);
        }

        #endregion

        #region Events linked to permission

        public void StartAddPermission(string request)
        {
            var evt = new Event
            {
                Id = 9,
                Task = Tasks.Permission,
                Message = $"Start to add permission : {request}"
            };

            LogInformation(evt);
        }

        public void FinishAddPermission(string request)
        {
            var evt = new Event
            {
                Id = 10,
                Task = Tasks.Permission,
                Message = $"Finish to add permission : {request}"
            };

            LogInformation(evt);
        }

        #endregion

        #region Events linked to authorization policy

        public void StartAddingAuthorizationPolicy(string request)
        {
            var evt = new Event
            {
                Id = 11,
                Task = Tasks.AuthorizationPolicy,
                Message = $"Start adding authorization policy : {request}"
            };

            LogInformation(evt);
        }

        public void FinishToAddAuthorizationPolicy(string result)
        {
            var evt = new Event
            {
                Id = 12,
                Task = Tasks.AuthorizationPolicy,
                Message = $"Finish to add authorization policy : {result}"
            };

            LogInformation(evt);
        }

        public void StartToRemoveAuthorizationPolicy(string policyId)
        {
            var evt = new Event
            {
                Id = 13,
                Task = Tasks.AuthorizationPolicy,
                Message = $"Start to remove authorization policy : {policyId}"
            };

            LogInformation(evt);
        }

        public void FinishToRemoveAuthorizationPolicy(string policyId)
        {
            var evt = new Event
            {
                Id = 14,
                Task = Tasks.AuthorizationPolicy,
                Message = $"Finish to remove authorization policy : {policyId}"
            };

            LogInformation(evt);
        }

        #endregion

        #region Events linked to resource set

        public void StartToAddResourceSet(string request)
        {
            var evt = new Event
            {
                Id = 15,
                Task = Tasks.ResourceSet,
                Message = $"Start to add resource set : {request}"
            };

            LogInformation(evt);
        }

        public void FinishToAddResourceSet(string result)
        {
            var evt = new Event
            {
                Id = 16,
                Task = Tasks.ResourceSet,
                Message = $"Finish to add resource set : {result}"
            };

            LogInformation(evt);
        }

        public void StartToRemoveResourceSet(string resourceSetId)
        {
            var evt = new Event
            {
                Id = 17,
                Task = Tasks.ResourceSet,
                Message = $"Start to remove resource set : {resourceSetId}"
            };

            LogInformation(evt);
        }

        public void FinishToRemoveResourceSet(string resourceSetId)
        {
            var evt = new Event
            {
                Id = 18,
                Task = Tasks.ResourceSet,
                Message = $"Finish to remove resource set : {resourceSetId}"
            };

            LogInformation(evt);
        }

        #endregion

        #region Events linked to scope

        public void StartToAddScope(string request)
        {
            var evt = new Event
            {
                Id = 19,
                Task = Tasks.Scope,
                Message = $"Start to add scope: {request}"
            };

            LogInformation(evt);
        }

        public void FinishToAddScope(string result)
        {
            var evt = new Event
            {
                Id = 20,
                Task = Tasks.Scope,
                Message = $"Finish to add scope: {result}"
            };

            LogInformation(evt);
        }

        public void StartToRemoveScope(string scope)
        {
            var evt = new Event
            {
                Id = 21,
                Task = Tasks.Scope,
                Message = $"Start to remove scope: {scope}"
            };

            LogInformation(evt);
        }

        public void FinishToRemoveScope(string scope)
        {
            var evt = new Event
            {
                Id = 22,
                Task = Tasks.Scope,
                Message = $"Finish to remove scope: {scope}"
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

        #endregion
    }
}
