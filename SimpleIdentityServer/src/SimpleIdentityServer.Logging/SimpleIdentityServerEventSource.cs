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

using Serilog;

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

        void StartGetTokenByClientCredentials(
            string scope);

        void EndGetTokenByClientCredentials(
            string clientId,
            string scope);

        void StartRevokeToken(string token);

        void EndRevokeToken(string token);

        #endregion

        void AuthenticateResourceOwner(string subject);

        void GiveConsent(string subject, 
            string clientId,
            string consentId);

        void OpenIdFailure(string code, 
            string description, 
            string state);

        void Failure(string message);

        void Info(string message);

        #region Events linked to the registration process

        void StartRegistration(string clientName);

        void EndRegistration(
            string clientId,
            string clientName);

        #endregion
    }
    
    public class SimpleIdentityServerEventSource : ISimpleIdentityServerEventSource
    {
        private readonly ILogger _logger;
                
        #region Constructor

        public SimpleIdentityServerEventSource(ILogger logger)
        {
            _logger = logger;
        }

        #endregion

        #region Events linked to the authorization process

        public void StartAuthorization(
            string clientId, 
            string responseType,
            string scope, 
            string individualClaims)
        {
            _logger.Information($"Start the authorization process for the client : {clientId}, response type : {responseType}, scope : {scope} and claims : {individualClaims}");
        }

        public void StartAuthorizationCodeFlow(
            string clientId,
            string scope,
            string individualClaims)
        {
            _logger.Information($"Start the authorization code flow for the client : {clientId}, scope : {scope} and claims : {individualClaims}");
        }

        public void StartProcessingAuthorizationRequest(string jsonAuthorizationRequest)
        {
            _logger.Information($"Start processing the authorization request : {jsonAuthorizationRequest}");
        }

        public void EndProcessingAuthorizationRequest(
            string jsonAuthorizationRequest,
            string actionType,
            string actionName)
        {
            _logger.Information($"End processing the authorization request, request : {jsonAuthorizationRequest}, action type : {actionType} and action name : {actionName}");
        }

        public void StartGeneratingAuthorizationResponseToClient(
            string clientId,
            string responseTypes)
        {
            _logger.Information($"Start to generate an authorization response for the client {clientId}, response types : {responseTypes}");
        }

        public void GrantAuthorizationCodeToClient(
            string clientId,
            string authorizationCode,
            string scopes)
        {
            _logger.Information($"Grant authorization code to the client {clientId}, authorization code : {authorizationCode} and scopes : {scopes}");
        }

        public void EndGeneratingAuthorizationResponseToClient(
            string clientId,
            string parameters)
        {
            _logger.Information($"Finished to generate the authorization response for the client {clientId}, parameters : {parameters}");
        }

        public void EndAuthorizationCodeFlow(string clientId, string actionType, string actionName)
        {
            _logger.Information($"End of the authorization code flow, client : {clientId}, action type : {actionType}, action name : {actionName}");
        }

        public void EndAuthorization(
            string actionType,
            string actionName,
            string parameters)
        {
            _logger.Information($"End the authorization process, action type : {actionType}, action name : {actionName} and parameters : {parameters}");
        }

        public void StartImplicitFlow(
            string clientId, 
            string scope, 
            string individualClaims)
        {
            _logger.Information($"Start the implicit flow, client : {clientId}, scope : {scope} and claims : {individualClaims}");
        }

        public void EndImplicitFlow(
            string clientId, 
            string actionType, 
            string actionName)
        {
            _logger.Information($"End the implicit flow, client : {clientId}, action type : {actionType} and action name : {actionName}");
        }

        public void StartHybridFlow(
            string clientId, 
            string scope, 
            string individualClaims)
        {
            _logger.Information($"Start the hybrid flow, client : {clientId}, scope : {scope} and claims : {individualClaims}");
        }

        public void EndHybridFlow(string clientId, string actionType, string actionName)
        {
            _logger.Information($"End the hybrid flow : {clientId}, action type : {actionType} and action name : {actionName}");
        }

        #endregion

        #region Events linked to the token endpoint
        
        public void StartGetTokenByResourceOwnerCredentials(string clientId, string userName, string password)
        {
            _logger.Information($"Start resource owner credentials grant-type, client : {clientId}, user name : {userName}, password : {password}");
        }

        public void EndGetTokenByResourceOwnerCredentials(string accessToken, string identityToken)
        {
            _logger.Information($"End of the resource owner credentials grant-type, access token : {accessToken}, identity token : {identityToken}");
        }

        public void StartGetTokenByAuthorizationCode(
            string clientId, 
            string authorizationCode)
        {
            _logger.Information($"Start authorization code grant-type, client : {clientId} and authorization code : {authorizationCode}");
        }

        public void EndGetTokenByAuthorizationCode(string accessToken, string identityToken)
        {
            _logger.Information($"End of the authorization code grant-type, access token : {accessToken}, identity token : {identityToken}");
        }

        public void StartToAuthenticateTheClient(string clientId, 
            string authenticationType)
        {
            _logger.Information($"Start to authenticate the client, client : {clientId}, authentication type : {authenticationType}");
        }
        
        public void FinishToAuthenticateTheClient(string clientId,
            string authenticationType)
        {
            _logger.Information($"Finish to authenticate the client, client : {clientId}, authentication type : {authenticationType}");
        }

        public void StartGetTokenByRefreshToken(
            string clientId, 
            string refreshToken)
        {
            _logger.Information($"Start refresh token grant-type, client : {clientId}, refresh token : {refreshToken}");
        }
        
        public void EndGetTokenByRefreshToken(string accessToken, string identityToken)
        {
            _logger.Information($"End refresh token grant-type, access token : {accessToken}, identity token : {identityToken}");
        }

        public void StartGetTokenByClientCredentials(string scope)
        {
            _logger.Information($"Start get token by client credentials, scope : {scope}");
        }

        public void EndGetTokenByClientCredentials(string clientId, string scope)
        {
            _logger.Information($"End get token by client credentials, client : {clientId}, scope : {scope}");
        }
        
        public void StartRevokeToken(string token)
        {
            _logger.Information($"Start revoking token, token : {token}");
        }

        public void EndRevokeToken(string token)
        {
            _logger.Information($"End revoking token, token : {token}");
        }

        #endregion

        #region Failing events

        public void OpenIdFailure(string code, 
            string description, 
            string state)
        {
            _logger.Error($"Something goes wrong in the open-id process, code : {code}, description : {description}, state : {state}");
        }
        
        public void Failure(string message)
        {
            _logger.Error($"Something goes wrong, code : {message}");
        }

        public void Info(string message)
        {
            _logger.Information(message);
        }

        #endregion

        #region Other events

        public void GrantAccessToClient(string clientId,
            string accessToken,
            string scopes)
        {
            _logger.Information($"Grant access to the client {clientId}, access token : {accessToken}, scopes : {scopes}");
        }

        public void AuthenticateResourceOwner(string subject)
        {
            _logger.Information($"The resource owner is authenticated {subject}");
        }
        
        public void GiveConsent(string subject,
            string clientId,
            string consentId)
        {
            _logger.Information($"The consent has been given by the resource owner, subject : {subject}, client id : {clientId}, consent id : {consentId}");
        }

        public void StartRegistration(string clientName)
        {
            _logger.Information($"Start the registration process, client name : {clientName}");
        }

        public void EndRegistration(
            string clientId,
            string clientName)
        {
            _logger.Information($"End the registration process, client id : {clientId}, client name : {clientName}");
        }

        #endregion
    }
}
