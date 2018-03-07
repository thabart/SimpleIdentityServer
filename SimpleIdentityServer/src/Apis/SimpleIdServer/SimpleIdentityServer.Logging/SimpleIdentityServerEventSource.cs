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

        void StartGetTokenByRefreshToken(string refreshToken);

        void EndGetTokenByRefreshToken(
            string accessToken,
            string identityToken);

        void StartGetTokenByClientCredentials(
            string scope);

        void EndGetTokenByClientCredentials(
            string clientId,
            string scope);

        void StartRevokeToken(string token);

        void EndRevokeToken(string token);

        #endregion

        void GiveConsent(string subject, 
            string clientId,
            string consentId);

        void OpenIdFailure(string code, 
            string description, 
            string state);

        void Failure(string message);

        void Failure(Exception exception);

        void Info(string message);

        #region Events linked to the registration process

        void StartRegistration(string clientName);

        void EndRegistration(
            string clientId,
            string clientName);

        #endregion

        #region Event linked to Authentication

        void AuthenticateResourceOwner(string subject);

        void GetConfirmationCode(string code);

        void InvalidateConfirmationCode(string code);

        void ConfirmationCodeNotValid(string code);

        #endregion
    }

    public class SimpleIdentityServerEventSource : ISimpleIdentityServerEventSource
    {
        private static class Tasks
        {
            public const string Authorization = "Authorization";
            public const string Token = "Token";
            public const string Failure = "Failure";
            public const string Information = "Info";
            public const string Other = "Other";
            public const string Authentication = "Authentication";
        }

        private readonly ILogger _logger;

        private const string MessagePattern = "{Id} : {Task}, {Message} : {Operation}";

        #region Constructor

        public SimpleIdentityServerEventSource(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SimpleIdentityServerEventSource>();
        }

        #endregion

        #region Events linked to the authorization process

        public void StartAuthorization(
            string clientId, 
            string responseType,
            string scope, 
            string individualClaims)
        {
            var evt = new Event
            {
                Id = 1,
                Task = Tasks.Authorization,
                Message = $"Start the authorization process for the client : {clientId}, response type : {responseType}, scope : {scope} and claims : {individualClaims}"
            };

            LogInformation(evt);
        }

        public void StartAuthorizationCodeFlow(
            string clientId,
            string scope,
            string individualClaims)
        {
            var evt = new Event
            {
                Id = 2,
                Task = Tasks.Authorization,
                Message = $"Start the authorization code flow for the client : {clientId}, scope : {scope} and claims : {individualClaims}",
                Operation = "authorization code"
            };

            LogInformation(evt);
        }

        public void StartProcessingAuthorizationRequest(string jsonAuthorizationRequest)
        {
            var evt = new Event
            {
                Id = 3,
                Task = Tasks.Authorization,
                Message = $"Start processing the authorization request : {jsonAuthorizationRequest}"
            };

            LogInformation(evt);
        }

        public void EndProcessingAuthorizationRequest(
            string jsonAuthorizationRequest,
            string actionType,
            string actionName)
        {
            var evt = new Event
            {
                Id = 4,
                Task = Tasks.Authorization,
                Message = $"End processing the authorization request, request : {jsonAuthorizationRequest}, action type : {actionType} and action name : {actionName}"
            };

            LogInformation(evt);
        }

        public void StartGeneratingAuthorizationResponseToClient(
            string clientId,
            string responseTypes)
        {
            var evt = new Event
            {
                Id = 5,
                Task = Tasks.Authorization,
                Message = $"Start to generate an authorization response for the client {clientId}, response types : {responseTypes}"
            };

            LogInformation(evt);
        }

        public void GrantAuthorizationCodeToClient(
            string clientId,
            string authorizationCode,
            string scopes)
        {
            var evt = new Event
            {
                Id = 6,
                Task = Tasks.Authorization,
                Message = $"Grant authorization code to the client {clientId}, authorization code : {authorizationCode} and scopes : {scopes}"
            };
            LogInformation(evt);
        }

        public void EndGeneratingAuthorizationResponseToClient(
            string clientId,
            string parameters)
        {
            var evt = new Event
            {
                Id = 7,
                Task = Tasks.Authorization,
                Message = $"Finished to generate the authorization response for the client {clientId}, parameters : {parameters}"
            };

            LogInformation(evt);
        }

        public void EndAuthorizationCodeFlow(string clientId, string actionType, string actionName)
        {
            var evt = new Event
            {
                Id = 8,
                Task = Tasks.Authorization,
                Message = $"End of the authorization code flow, client : {clientId}, action type : {actionType}, action name : {actionName}",
                Operation = "authorization code"
            };

            LogInformation(evt);
        }

        public void EndAuthorization(
            string actionType,
            string actionName,
            string parameters)
        {
            var evt = new Event
            {
                Id = 9,
                Task = Tasks.Authorization,
                Message = $"End the authorization process, action type : {actionType}, action name : {actionName} and parameters : {parameters}"
            };

            LogInformation(evt);
        }

        public void StartImplicitFlow(
            string clientId, 
            string scope, 
            string individualClaims)
        {
            var evt = new Event
            {
                Id = 10,
                Task = Tasks.Authorization,
                Message = $"Start the implicit flow, client : {clientId}, scope : {scope} and claims : {individualClaims}",
                Operation = "implicit"
            };

            LogInformation(evt);
        }

        public void EndImplicitFlow(
            string clientId, 
            string actionType, 
            string actionName)
        {
            var evt = new Event
            {
                Id = 11,
                Task = Tasks.Authorization,
                Message = $"End the implicit flow, client : {clientId}, action type : {actionType} and action name : {actionName}",
                Operation = "implicit"
            };

            LogInformation(evt);
        }

        public void StartHybridFlow(
            string clientId, 
            string scope, 
            string individualClaims)
        {
            var evt = new Event
            {
                Id = 12,
                Task = Tasks.Authorization,
                Message = $"Start the hybrid flow, client : {clientId}, scope : {scope} and claims : {individualClaims}",
                Operation = "hybrid"
            };

            LogInformation(evt);
        }

        public void EndHybridFlow(string clientId, string actionType, string actionName)
        {
            var evt = new Event
            {
                Id = 13,
                Task = Tasks.Authorization,
                Message = $"End the hybrid flow : {clientId}, action type : {actionType} and action name : {actionName}",
                Operation = "hybrid"
            };

            LogInformation(evt);
        }

        #endregion

        #region Events linked to the token endpoint
        
        public void StartGetTokenByResourceOwnerCredentials(string clientId, string userName, string password)
        {
            var evt = new Event
            {
                Id = 14,
                Task = Tasks.Token,
                Message = $"Start resource owner credentials grant-type, client : {clientId}, user name : {userName}, password : {password}",
                Operation = "resource owner credentials"
            };

            LogInformation(evt);
        }

        public void EndGetTokenByResourceOwnerCredentials(string accessToken, string identityToken)
        {
            var evt = new Event
            {
                Id = 15,
                Task = Tasks.Token,
                Message = $"End of the resource owner credentials grant-type, access token : {accessToken}, identity token : {identityToken}",
                Operation = "resource owner credentials"
            };

            LogInformation(evt);
        }

        public void StartGetTokenByAuthorizationCode(
            string clientId, 
            string authorizationCode)
        {
            var evt = new Event
            {
                Id = 16,
                Task = Tasks.Token,
                Message = $"Start authorization code grant-type, client : {clientId} and authorization code : {authorizationCode}",
                Operation = "authorization code"
            };

            LogInformation(evt);
        }

        public void EndGetTokenByAuthorizationCode(string accessToken, string identityToken)
        {
            var evt = new Event
            {
                Id = 17,
                Task = Tasks.Token,
                Message = $"End of the authorization code grant-type, access token : {accessToken}, identity token : {identityToken}",
                Operation = "authorization code"
            };

            LogInformation(evt);
        }

        public void StartToAuthenticateTheClient(string clientId, 
            string authenticationType)
        {
            var evt = new Event
            {
                Id = 18,
                Task = Tasks.Token,
                Message = $"Start to authenticate the client, client : {clientId}, authentication type : {authenticationType}"
            };

            LogInformation(evt);
        }
        
        public void FinishToAuthenticateTheClient(string clientId,
            string authenticationType)
        {
            var evt = new Event
            {
                Id = 19,
                Task = Tasks.Token,
                Message = $"Finish to authenticate the client, client : {clientId}, authentication type : {authenticationType}"
            };

            LogInformation(evt);
        }

        public void StartGetTokenByRefreshToken(string refreshToken)
        {
            var evt = new Event
            {
                Id = 20,
                Task = Tasks.Token,
                Message = $"Start refresh token grant-type, refresh token : {refreshToken}",
                Operation = "refresh token"
            };

            LogInformation(evt);
        }
        
        public void EndGetTokenByRefreshToken(string accessToken, string identityToken)
        {
            var evt = new Event
            {
                Id = 21,
                Task = Tasks.Token,
                Message = $"End refresh token grant-type, access token : {accessToken}, identity token : {identityToken}",
                Operation = "refresh token"
            };

            LogInformation(evt);
        }

        public void StartGetTokenByClientCredentials(string scope)
        {
            var evt = new Event
            {
                Id = 22,
                Task = Tasks.Token,
                Message = $"Start get token by client credentials, scope : {scope}",
                Operation = "client credentials"
            };

            LogInformation(evt);
        }

        public void EndGetTokenByClientCredentials(string clientId, string scope)
        {
            var evt = new Event
            {
                Id = 23,
                Task = Tasks.Token,
                Message = $"End get token by client credentials, client : {clientId}, scope : {scope}",
                Operation = "client credentials"
            };

            LogInformation(evt);
        }
        
        public void StartRevokeToken(string token)
        {
            var evt = new Event
            {
                Id = 24,
                Task = Tasks.Token,
                Message = $"Start revoking token, token : {token}"
            };
            LogInformation(evt);
        }

        public void EndRevokeToken(string token)
        {
            var evt = new Event
            {
                Id = 25,
                Task = Tasks.Token,
                Message = $"End revoking token, token : {token}"
            };
            LogInformation(evt);
        }

        #endregion

        #region Failing events

        public void OpenIdFailure(string code, 
            string description, 
            string state)
        {
            var evt = new Event
            {
                Id = 26,
                Task = Tasks.Failure,
                Message = $"Something goes wrong in the open-id process, code : {code}, description : {description}, state : {state}"
            };

            LogError(evt);
        }
        
        public void Failure(string message)
        {
            var evt = new Event
            {
                Id = 27,
                Task = Tasks.Failure,
                Message = $"Something goes wrong, code : {message}"
            };

            LogError(evt);
        }

        public void Failure(Exception exception)
        {
            var evt = new Event
            {
                Id = 28,
                Task = Tasks.Failure,
                Message = "an error occured"
            };

            LogError(evt, new EventId(28), exception);
        }

        public void Info(string message)
        {
            var evt = new Event
            {
                Id = 29,
                Task = Tasks.Information,
                Message = message
            };

            LogInformation(evt);
        }

        #endregion

        #region Other events

        public void GrantAccessToClient(string clientId,
            string accessToken,
            string scopes)
        {
            var evt = new Event
            {
                Id = 30,
                Task = Tasks.Other,
                Message = $"Grant access to the client {clientId}, access token : {accessToken}, scopes : {scopes}"
            };
            LogInformation(evt);
        }

        public void AuthenticateResourceOwner(string subject)
        {
            var evt = new Event
            {
                Id = 31,
                Task = Tasks.Other,
                Message = $"The resource owner is authenticated {subject}"
            };
            LogInformation(evt);
        }
        
        public void GiveConsent(string subject,
            string clientId,
            string consentId)
        {
            var evt = new Event
            {
                Id = 32,
                Task = Tasks.Other,
                Message = $"The consent has been given by the resource owner, subject : {subject}, client id : {clientId}, consent id : {consentId}"
            };
            LogInformation(evt);
        }

        public void StartRegistration(string clientName)
        {
            var evt = new Event
            {
                Id = 33,
                Task = Tasks.Other,
                Message = $"Start the registration process, client name : {clientName}"
            };
            LogInformation(evt);
        }

        public void EndRegistration(
            string clientId,
            string clientName)
        {
            var evt = new Event
            {
                Id = 34,
                Task = Tasks.Other,
                Message = $"End the registration process, client id : {clientId}, client name : {clientName}"
            };
            LogInformation(evt);
        }

        #endregion

        #region Events linked to Authentication

        public void GetConfirmationCode(string code)
        {
            var evt = new Event
            {
                Id = 35,
                Task = Tasks.Authentication,
                Message = $"Get confirmation code {code}"
            };
            LogInformation(evt);
        }

        public void InvalidateConfirmationCode(string code)
        {
            var evt = new Event
            {
                Id = 36,
                Task = Tasks.Authentication,
                Message = $"Remove confirmation code {code}"
            };
            LogInformation(evt);
        }

        public void ConfirmationCodeNotValid(string code)
        {
            var evt = new Event
            {
                Id = 37,
                Task = Tasks.Authentication,
                Message = $"Confirmation code is not valid {code}"
            };
            LogError(evt);
        }

        #endregion

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
