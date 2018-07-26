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

namespace SimpleIdentityServer.Uma.Logging
{
    public interface IUmaServerEventSource : IEventSource
    {
        #region Events linked to the authorization

        void StartGettingAuthorization(string request);

        void CheckAuthorizationPolicy(string request);

        void RequestIsNotAuthorized(string request);

        void RequestIsAuthorized(string request);

        void AuthorizationPoliciesFailed(string ticketId);

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
        void StartToUpdateResourceSet(string request);
        void FinishToUpdateResourceSet(string request);

        #endregion

        #region Events linked to scope

        void StartToAddScope(string request);

        void FinishToAddScope(string result);

        void StartToRemoveScope(string scope);

        void FinishToRemoveScope(string scope);

        #endregion
    }

    public class UmaServerEventSource : BaseEventSource, IUmaServerEventSource
    {
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

        public UmaServerEventSource(ILoggerFactory loggerFactory) : base(loggerFactory.CreateLogger<UmaServerEventSource>())
        {
        }

        #endregion

        #region Events linked to the authorization

        public void StartGettingAuthorization(string request)
        {
            var evt = new Event
            {
                Id = 700,
                Task = Tasks.Authorization,
                Message = $"Start getting RPT tokens : {request}"
            };

            LogInformation(evt);
        }

        public void CheckAuthorizationPolicy(string request)
        {
            var evt = new Event
            {
                Id = 701,
                Task = Tasks.Authorization,
                Message = $"Check authorization policy : {request}"
            };

            LogInformation(evt);
        }

        public void AuthorizationPoliciesFailed(string ticketId)
        {
            var evt = new Event
            {
                Id = 702,
                Task = Tasks.Authorization,
                Message = $"The authorization policies failed for the ticket {ticketId}"
            };

            LogInformation(evt);
        }

        public void RequestIsNotAuthorized(string request)
        {
            var evt = new Event
            {
                Id = 703,
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
                Id = 704,
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
                Id = 710,
                Task = Tasks.Introspection,
                Message = $"Start to introspect the RPT {rpt}"
            };

            LogInformation(evt);
        }

        public void RptHasExpired(string rpt)
        {
            var evt = new Event
            {
                Id = 711,
                Task = Tasks.Introspection,
                Message = $"RPT {rpt} has expired"
            };

            LogInformation(evt);
        }

        public void EndIntrospection(string result)
        {
            var evt = new Event
            {
                Id = 712,
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
                Id = 720,
                Task = Tasks.Permission,
                Message = $"Start to add permission : {request}"
            };

            LogInformation(evt);
        }

        public void FinishAddPermission(string request)
        {
            var evt = new Event
            {
                Id = 721,
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
                Id = 730,
                Task = Tasks.AuthorizationPolicy,
                Message = $"Start adding authorization policy : {request}"
            };

            LogInformation(evt);
        }

        public void FinishToAddAuthorizationPolicy(string result)
        {
            var evt = new Event
            {
                Id = 731,
                Task = Tasks.AuthorizationPolicy,
                Message = $"Finish to add authorization policy : {result}"
            };

            LogInformation(evt);
        }

        public void StartToRemoveAuthorizationPolicy(string policyId)
        {
            var evt = new Event
            {
                Id = 732,
                Task = Tasks.AuthorizationPolicy,
                Message = $"Start to remove authorization policy : {policyId}"
            };

            LogInformation(evt);
        }

        public void FinishToRemoveAuthorizationPolicy(string policyId)
        {
            var evt = new Event
            {
                Id = 733,
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
                Id = 740,
                Task = Tasks.ResourceSet,
                Message = $"Start to add resource set : {request}"
            };

            LogInformation(evt);
        }

        public void FinishToAddResourceSet(string result)
        {
            var evt = new Event
            {
                Id = 741,
                Task = Tasks.ResourceSet,
                Message = $"Finish to add resource set : {result}"
            };

            LogInformation(evt);
        }

        public void StartToRemoveResourceSet(string resourceSetId)
        {
            var evt = new Event
            {
                Id = 742,
                Task = Tasks.ResourceSet,
                Message = $"Start to remove resource set : {resourceSetId}"
            };

            LogInformation(evt);
        }

        public void FinishToRemoveResourceSet(string resourceSetId)
        {
            var evt = new Event
            {
                Id = 743,
                Task = Tasks.ResourceSet,
                Message = $"Finish to remove resource set : {resourceSetId}"
            };

            LogInformation(evt);
        }

        public void StartToUpdateResourceSet(string request)
        {
            var evt = new Event
            {
                Id = 744,
                Task = Tasks.ResourceSet,
                Message = $"Start to update the resource set : {request}"
            };

            LogInformation(evt);
        }

        public void FinishToUpdateResourceSet(string request)
        {
            var evt = new Event
            {
                Id = 745,
                Task = Tasks.ResourceSet,
                Message = $"Start to update the resource set : {request}"
            };

            LogInformation(evt);
        }

        #endregion

        #region Events linked to scope

        public void StartToAddScope(string request)
        {
            var evt = new Event
            {
                Id = 750,
                Task = Tasks.Scope,
                Message = $"Start to add scope: {request}"
            };

            LogInformation(evt);
        }

        public void FinishToAddScope(string result)
        {
            var evt = new Event
            {
                Id = 751,
                Task = Tasks.Scope,
                Message = $"Finish to add scope: {result}"
            };

            LogInformation(evt);
        }

        public void StartToRemoveScope(string scope)
        {
            var evt = new Event
            {
                Id = 752,
                Task = Tasks.Scope,
                Message = $"Start to remove scope: {scope}"
            };

            LogInformation(evt);
        }

        public void FinishToRemoveScope(string scope)
        {
            var evt = new Event
            {
                Id = 753,
                Task = Tasks.Scope,
                Message = $"Finish to remove scope: {scope}"
            };

            LogInformation(evt);
        }

        #endregion
    }
}
