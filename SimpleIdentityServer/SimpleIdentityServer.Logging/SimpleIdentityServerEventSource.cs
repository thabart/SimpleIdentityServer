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

            WriteEvent(Constants.EventIds.AuthorizationCodeFlowStarted, jsonAuthorizationRequest);
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
