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

        #endregion

        void AuthenticateResourceOwner(string subject);

        void OpenIdFailure(string code, 
            string description, 
            string state);

        void Failure(string message);
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

        #endregion
    }
}
