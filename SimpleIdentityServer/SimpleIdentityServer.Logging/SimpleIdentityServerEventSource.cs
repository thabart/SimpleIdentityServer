using System;
using System.Diagnostics.Tracing;

namespace SimpleIdentityServer.Logging
{
    public interface ISimpleIdentityServerEventSource
    {
        void StartAuthorization(
            string clientId,
            string responseType,
            string scope,
            string individualClaims);

        void EndAuthorization(
            string actionType,
            string controllerAction);

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

        #region Events related to the authorization process

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

        [Event(Constants.EventIds.AuthorizationEnded,
            Level = EventLevel.Informational,
            Message = "end the authorization process",
            Opcode = EventOpcode.Stop,
            Task = Constants.Tasks.Authorization)]
        public void EndAuthorization(
            string actionType,
            string actionName)
        {
            if (!IsEnabled())
            {
                return;
            }

            WriteEvent(Constants.EventIds.AuthorizationEnded, actionType, actionName);
        }

        #endregion

        #region Failing events


        [Event(Constants.EventIds.OpenIdFailure,
            Level = EventLevel.Error,
            Message = "something goes wrong in the open-id process",
            Opcode = EventOpcode.Info,
            Task = Constants.Tasks.Failure)]
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
