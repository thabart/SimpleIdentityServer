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

namespace SimpleIdentityServer.OAuth.Logging
{
    public interface IOAuthEventSource : IEventSource
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

        void GrantAccessToClient(string clientId, string accessToken, string scopes);

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

        #region Event linked to client registration

        void StartRegistration(string clientName);
        void EndRegistration(string clientId, string clientName);

        #endregion

        void OAuthFailure(string code, 
            string description, 
            string state);
    }

    public class OAuthEventSource : BaseEventSource, IOAuthEventSource
    {
        private static class Tasks
        {
            public const string Authorization = "Authorization";
            public const string Token = "Token";
            public const string ClientRegistration = "ClientRegistration";
        }

        #region Constructor

        public OAuthEventSource(ILoggerFactory loggerFactory) : base(loggerFactory.CreateLogger<OAuthEventSource>())
        {
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

        public void GrantAccessToClient(string clientId,
            string accessToken,
            string scopes)
        {
            var evt = new Event
            {
                Id = 14,
                Task = Tasks.Authorization,
                Message = $"Grant access to the client {clientId}, access token : {accessToken}, scopes : {scopes}"
            };
            LogInformation(evt);
        }

        #endregion

        #region Events linked to the token endpoint

        public void StartGetTokenByResourceOwnerCredentials(string clientId, string userName, string password)
        {
            var evt = new Event
            {
                Id = 15,
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
                Id = 16,
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
                Id = 17,
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
                Id = 18,
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
                Id = 19,
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
                Id = 20,
                Task = Tasks.Token,
                Message = $"Finish to authenticate the client, client : {clientId}, authentication type : {authenticationType}"
            };

            LogInformation(evt);
        }

        public void StartGetTokenByRefreshToken(string refreshToken)
        {
            var evt = new Event
            {
                Id = 21,
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
                Id = 22,
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
                Id = 23,
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
                Id = 24,
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
                Id = 25,
                Task = Tasks.Token,
                Message = $"Start revoking token, token : {token}"
            };
            LogInformation(evt);
        }

        public void EndRevokeToken(string token)
        {
            var evt = new Event
            {
                Id = 26,
                Task = Tasks.Token,
                Message = $"End revoking token, token : {token}"
            };
            LogInformation(evt);
        }

        #endregion

        #region Client registration

        public void StartRegistration(string clientName)
        {
            var evt = new Event
            {
                Id = 27,
                Task = Tasks.ClientRegistration,
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
                Id = 28,
                Task = Tasks.ClientRegistration,
                Message = $"End the registration process, client id : {clientId}, client name : {clientName}"
            };
            LogInformation(evt);
        }

        #endregion

        #region Failing events

        public void OAuthFailure(string code,
            string description,
            string state)
        {
            var evt = new Event
            {
                Id = 100,
                Task = EventTasks.Failure,
                Message = $"Something goes wrong in the oauth process, code : {code}, description : {description}, state : {state}"
            };

            LogError(evt);
        }

        #endregion
    }
}
