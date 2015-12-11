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

        void EndAuthorization(
            string actionType,
            string controllerAction,
            string parameters);

        #endregion

        void OpenIdFailure(string code, 
            string description, 
            string state);
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

        [Event(Constants.EventIds.GrantAccessToClient,
            Level = EventLevel.Informational,
            Message = "grant access to the client {0}",
            Opcode = EventOpcode.Info,
            Task = Constants.Tasks.Authorization)]
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

        #endregion
    }
}
