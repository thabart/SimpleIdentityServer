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

using System;
using System.Diagnostics.Tracing;

namespace SimpleIdentityServer.Logging
{
    public interface ISimpleIdentityServerEventSource
    {
        #region Events linked to the authorization process

        void StartAuthorization(
            string clientId,
            string responseType,
            string scope,
            string individualClaims);

        void StartAuthorizationCodeFlow(
            string clientId,
            string scope,
            string individualClaims);

        void StartProcessingAuthorizationRequest(
            string jsonAuthorizationRequest);

        void EndProcessingAuthorizationRequest(
            string jsonAuthorizationRequest,
            string actionType,
            string actionName);

        void StartGeneratingAuthorizationResponseToClient(
            string clientId,
            string responseTypes);

        void GrantAccessToClient(
            string clientId,
            string accessToken,
            string scopes);

        void GrantAuthorizationCodeToClient(
            string clientId,
            string authorizationCode,
            string scopes);

        void EndGeneratingAuthorizationResponseToClient(
            string clientId,
            string parameters);

        void EndAuthorizationCodeFlow(
            string clientId,
            string actionType,
            string actionName);

        void StartImplicitFlow(
            string clientId,
            string scope,
            string individualClaims);

        void EndImplicitFlow(
            string clientId,
            string actionType,
            string actionName);

        void StartHybridFlow(
            string clientId,
            string scope,
            string individualClaims);

        void EndHybridFlow(
            string clientId,
            string actionType,
            string actionName);

        void EndAuthorization(
            string actionType,
            string controllerAction,
            string parameters);

        #endregion

        #region Events linked to the token process

        void StartGetTokenByResourceOwnerCredentials(
            string clientId, 
            string userName,
            string password);

        void EndGetTokenByResourceOwnerCredentials(
            string accessToken,
            string identityToken);

        void StartGetTokenByAuthorizationCode(
            string clientId,
            string authorizationCode);

        void EndGetTokenByAuthorizationCode(
            string accessToken,
            string identityToken);

        void StartToAuthenticateTheClient(
            string clientId,
            string authenticationType);

        void FinishToAuthenticateTheClient(
            string clientId,
            string authenticateType);

        void StartGetTokenByRefreshToken(
            string clientId,
            string refreshToken);

        void EndGetTokenByRefreshToken(
            string accessToken,
            string identityToken);

        #endregion

        void AuthenticateResourceOwner(string subject);

        void GiveConsent(string subject, 
            string clientId,
            string consentId);

        void OpenIdFailure(string code, 
            string description, 
            string state);

        void Failure(string message);

        #region Events linked to the registration process

        void StartRegistration(string clientName);

        void EndRegistration(
            string clientId,
            string clientName);

        #endregion
    }

    [EventSource(Name = "SimpleIdentityServer")]
    public class SimpleIdentityServerEventSource : EventSource, ISimpleIdentityServerEventSource
    {
        private static readonly Lazy<SimpleIdentityServerEventSource> Instance = new Lazy<SimpleIdentityServerEventSource>(() => new SimpleIdentityServerEventSource());

        private SimpleIdentityServerEventSource()
        {
        }

        public static SimpleIdentityServerEventSource Log
        {
            get { return Instance.Value; }
        }

        #region Events linked to the authorization process

        [Event(Constants.EventIds.AuthorizationStarted, 
            Level = EventLevel.Informational, 
            Message = "start the authorization process",
            Opcode = EventOpcode.Start,
            Task = Constants.Tasks.Authorization)]
        public void StartAuthorization(
            string clientId, 
            string responseType,
            string scope, 
            string individualClaims)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.AuthorizationStarted, clientId, responseType, scope, individualClaims);
        }


        [Event(Constants.EventIds.AuthorizationCodeFlowStarted,
            Level = EventLevel.Informational,
            Message = "start the authorization code flow",
            Opcode = EventOpcode.Start,
            Task = Constants.Tasks.Authorization)]
        public void StartAuthorizationCodeFlow(
            string clientId,
            string scope,
            string individualClaims)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.AuthorizationCodeFlowStarted, clientId, scope, individualClaims);
        }

        [Event(Constants.EventIds.StartProcessingAuthorizationRequest,
            Level = EventLevel.Informational,
            Message = "start processing the authorization request",
            Opcode = EventOpcode.Start,
            Task = Constants.Tasks.Authorization)]
        public void StartProcessingAuthorizationRequest(string jsonAuthorizationRequest)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.StartProcessingAuthorizationRequest, jsonAuthorizationRequest);
        }

        [Event(Constants.EventIds.EndProcessingAuthorizationRequest,
            Level = EventLevel.Informational,
            Message = "end processing the authorization request",
            Opcode = EventOpcode.Stop,
            Task = Constants.Tasks.Authorization)]
        public void EndProcessingAuthorizationRequest(
            string jsonAuthorizationRequest,
            string actionType,
            string actionName)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.EndProcessingAuthorizationRequest, jsonAuthorizationRequest, actionType, actionName);
        }
        
        [Event(Constants.EventIds.StartGeneratingAuthorizationResponseToClient,
            Level = EventLevel.Informational,
            Message = "start to generate an authorization response for the client {0}",
            Opcode = EventOpcode.Start,
            Task = Constants.Tasks.Authorization)]
        public void StartGeneratingAuthorizationResponseToClient(
            string clientId,
            string responseTypes)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.StartGeneratingAuthorizationResponseToClient,
                clientId,
                responseTypes);
        }

        [Event(Constants.EventIds.GrantAuthorizationCodeToClient,
            Level = EventLevel.Informational,
            Message = "grant authorization code to the client {0}",
            Opcode = EventOpcode.Info,
            Task = Constants.Tasks.Authorization)]
        public void GrantAuthorizationCodeToClient(
            string clientId,
            string authorizationCode,
            string scopes)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.GrantAuthorizationCodeToClient,
                clientId,
                authorizationCode,
                scopes);
        }

        [Event(Constants.EventIds.EndGeneratingAuthorizationResponseToClient,
            Level = EventLevel.Informational,
            Message = "finished to generate the authorization response for the client {0}",
            Opcode = EventOpcode.Stop,
            Task = Constants.Tasks.Authorization)]
        public void EndGeneratingAuthorizationResponseToClient(
            string clientId,
            string parameters)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.EndGeneratingAuthorizationResponseToClient,
                clientId, 
                parameters);
        }


        [Event(Constants.EventIds.AuthorizationCodeFlowEnded,
            Level = EventLevel.Informational,
            Message = "end of the authorization code flow",
            Opcode = EventOpcode.Stop,
            Task = Constants.Tasks.Authorization)]
        public void EndAuthorizationCodeFlow(string clientId, string actionType, string actionName)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.AuthorizationCodeFlowEnded,
                clientId,
                actionType,
                actionName);
        }

        [Event(Constants.EventIds.AuthorizationEnded,
            Level = EventLevel.Informational,
            Message = "end the authorization process",
            Opcode = EventOpcode.Stop,
            Task = Constants.Tasks.Authorization)]
        public void EndAuthorization(
            string actionType,
            string actionName,
            string parameters)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.AuthorizationEnded, actionType, actionName, parameters);
        }

        [Event(Constants.EventIds.ImplicitFlowStart,
            Level = EventLevel.Informational,
            Message = "start the implicit flow",
            Opcode = EventOpcode.Start,
            Task = Constants.Tasks.Authorization)]
        public void StartImplicitFlow(
            string clientId, 
            string scope, 
            string individualClaims)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.ImplicitFlowStart, clientId, scope, individualClaims);
        }

        [Event(Constants.EventIds.ImplicitFlowEnd,
            Level = EventLevel.Informational,
            Message = "end the implicit flow",
            Opcode = EventOpcode.Stop,
            Task = Constants.Tasks.Authorization)]
        public void EndImplicitFlow(
            string clientId, 
            string actionType, 
            string actionName)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.ImplicitFlowEnd, clientId, actionType, actionName);
        }

        [Event(Constants.EventIds.HybridFlowStart,
            Level = EventLevel.Informational,
            Message = "start the hybrid flow",
            Opcode = EventOpcode.Start,
            Task = Constants.Tasks.Authorization)]
        public void StartHybridFlow(
            string clientId, 
            string scope, 
            string individualClaims)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.HybridFlowStart, clientId, scope, individualClaims);
        }

        [Event(Constants.EventIds.HybridFlowEnd,
            Level = EventLevel.Informational,
            Message = "end the hybrid flow",
            Opcode = EventOpcode.Stop,
            Task = Constants.Tasks.Authorization)]
        public void EndHybridFlow(string clientId, string actionType, string actionName)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.HybridFlowEnd, clientId, actionType, actionName);
        }

        #endregion

        #region Events linked to the token endpoint
        
        [Event(Constants.EventIds.StartResourceOwnerCredentialsGrantType,
            Level = EventLevel.Informational,
            Message = "start resource owner credentials grant-type",
            Opcode = EventOpcode.Start,
            Task = Constants.Tasks.Token)]
        public void StartGetTokenByResourceOwnerCredentials(string clientId, string userName, string password)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.StartResourceOwnerCredentialsGrantType, clientId, userName, password);
        }
        
        [Event(Constants.EventIds.EndResourceOwnerCredentialsGrantType,
            Level = EventLevel.Informational,
            Message = "end of the resource owner credentials grant-type",
            Opcode = EventOpcode.Stop,
            Task = Constants.Tasks.Token)]
        public void EndGetTokenByResourceOwnerCredentials(string accessToken, string identityToken)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.EndResourceOwnerCredentialsGrantType, accessToken, identityToken);
        }
        
        [Event(Constants.EventIds.StartAuthorizationCodeGrantType,
            Level = EventLevel.Informational,
            Message = "start authorization code grant-type",
            Opcode = EventOpcode.Start,
            Task = Constants.Tasks.Token)]
        public void StartGetTokenByAuthorizationCode(
            string clientId, 
            string authorizationCode)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.StartAuthorizationCodeGrantType, clientId, authorizationCode);
        }

        [Event(Constants.EventIds.EndAuthorizationCodeGrantType,
            Level = EventLevel.Informational,
            Message = "end of the authorization code grant-type",
            Opcode = EventOpcode.Stop,
            Task = Constants.Tasks.Token)]
        public void EndGetTokenByAuthorizationCode(string accessToken, string identityToken)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.EndAuthorizationCodeGrantType, accessToken, identityToken);
        }

        [Event(Constants.EventIds.StartToAuthenticateTheClient,
            Level = EventLevel.Informational,
            Message = "start to authenticate the client",
            Opcode = EventOpcode.Start,
            Task = Constants.Tasks.Token)]
        public void StartToAuthenticateTheClient(string clientId, 
            string authenticationType)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.StartToAuthenticateTheClient,
                clientId, 
                authenticationType);
        }

        [Event(Constants.EventIds.FinishToAuthenticateTheClient,
            Level = EventLevel.Informational,
            Message = "finish to authenticate the client",
            Opcode = EventOpcode.Stop,
            Task = Constants.Tasks.Token)]
        public void FinishToAuthenticateTheClient(string clientId,
            string authenticationType)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.FinishToAuthenticateTheClient,
                clientId,
                authenticationType);
        }

        [Event(Constants.EventIds.StartRefreshTokenGrantType,
            Level = EventLevel.Informational,
            Message = "start refresh token grant-type",
            Opcode = EventOpcode.Start,
            Task = Constants.Tasks.Token)]
        public void StartGetTokenByRefreshToken(
            string clientId, 
            string refreshToken)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.StartRefreshTokenGrantType,
                clientId,
                refreshToken);
        }


        [Event(Constants.EventIds.EndRefreshTokenGrantType,
            Level = EventLevel.Informational,
            Message = "end refresh token grant-type",
            Opcode = EventOpcode.Stop,
            Task = Constants.Tasks.Token)]
        public void EndGetTokenByRefreshToken(string accessToken, string identityToken)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.EndRefreshTokenGrantType,
                accessToken,
                identityToken);
        }

        #endregion

        #region Failing events
        
        [Event(Constants.EventIds.OpenIdFailure,
            Level = EventLevel.Error,
            Message = "something goes wrong in the open-id process")]
        public void OpenIdFailure(string code, 
            string description, 
            string state)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.OpenIdFailure, code, description, state);
        }

        [Event(Constants.EventIds.Failure,
            Level = EventLevel.Error,
            Message = "something goes wrong")]
        public void Failure(string message)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.Failure, message);
        }

        #endregion

        #region Other events
        
        [Event(Constants.EventIds.GrantAccessToClient,
            Level = EventLevel.Informational,
            Message = "grant access to the client {0}",
            Opcode = EventOpcode.Info,
            Task = Constants.Tasks.Grant)]
        public void GrantAccessToClient(string clientId,
            string accessToken,
            string scopes)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.GrantAccessToClient,
                clientId,
                accessToken,
                scopes);
        }

        [Event(Constants.EventIds.ResourceOwnerIsAuthenticated,
            Level = EventLevel.Informational,
            Message = "the resource owner is authenticated",
            Opcode = EventOpcode.Info,
            Task = Constants.Tasks.Authenticate)]
        public void AuthenticateResourceOwner(string subject)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.ResourceOwnerIsAuthenticated,
                subject);
        }

        [Event(Constants.EventIds.ConsentHasBeenGivenByResourceOwner,
            Level = EventLevel.Informational,
            Message = "the consent has been given by the resource owner",
            Opcode = EventOpcode.Info,
            Task = Constants.Tasks.Consent)]
        public void GiveConsent(string subject,
            string clientId,
            string consentId)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.ConsentHasBeenGivenByResourceOwner,
                subject,
                clientId,
                consentId);
        }

        [Event(Constants.EventIds.StartRegistration,
            Level = EventLevel.Informational,
            Message = "start the registration process",
            Opcode = EventOpcode.Start,
            Task = Constants.Tasks.Registration)]
        public void StartRegistration(string clientName)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.StartRegistration,
                clientName);
        }

        [Event(Constants.EventIds.EndRegistration,
            Level = EventLevel.Informational,
            Message = "end the registration process",
            Opcode = EventOpcode.Stop,
            Task = Constants.Tasks.Registration)]
        public void EndRegistration(
            string clientId,
            string clientName)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.EndRegistration,
                clientId,
                clientName);
        }

        #endregion
    }
}
