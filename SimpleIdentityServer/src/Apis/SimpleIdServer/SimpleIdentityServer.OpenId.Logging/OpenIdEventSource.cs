using Microsoft.Extensions.Logging;
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.OpenId.Logging
{
    public interface IOpenIdEventSource : IEventSource
    {
        #region User interaction

        void GiveConsent(string subject, string clientId, string consentId);
        void AuthenticateResourceOwner(string subject);
        void GetConfirmationCode(string code);
        void InvalidateConfirmationCode(string code);
        void ConfirmationCodeNotValid(string code);

        #endregion

        #region UserManagement

        void AddResourceOwner(string subject);

        #endregion

        void OpenIdFailure(string code,
            string description,
            string state);
    }

    public class OpenIdEventSource : BaseEventSource, IOpenIdEventSource
    {
        private static class Tasks
        {
            public const string UserInteraction = "UserInteraction";
            public const string UserManagement = "UserManagement";
        }

        public OpenIdEventSource(ILoggerFactory loggerFactory) : base(loggerFactory.CreateLogger<OpenIdEventSource>())
        {
        }

        #region User interaction
        
        public void AuthenticateResourceOwner(string subject)
        {
            var evt = new Event
            {
                Id = 201,
                Task = Tasks.UserInteraction,
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
                Id = 200,
                Task = Tasks.UserInteraction,
                Message = $"The consent has been given by the resource owner, subject : {subject}, client id : {clientId}, consent id : {consentId}"
            };
            LogInformation(evt);
        }

        public void GetConfirmationCode(string code)
        {
            var evt = new Event
            {
                Id = 202,
                Task = Tasks.UserInteraction,
                Message = $"Get confirmation code {code}"
            };
            LogInformation(evt);
        }

        public void InvalidateConfirmationCode(string code)
        {
            var evt = new Event
            {
                Id = 203,
                Task = Tasks.UserInteraction,
                Message = $"Remove confirmation code {code}"
            };
            LogInformation(evt);
        }

        public void ConfirmationCodeNotValid(string code)
        {
            var evt = new Event
            {
                Id = 204,
                Task = Tasks.UserInteraction,
                Message = $"Confirmation code is not valid {code}"
            };
            LogError(evt);
        }

        #endregion

        #region UserManagement

        public void AddResourceOwner(string subject)
        {
            var evt = new Event
            {
                Id = 205,
                Task = Tasks.UserManagement,
                Message = $"The resource owner is created {subject}"
            };
            LogInformation(evt);
        }

        #endregion

        #region Failures

        public void OpenIdFailure(string code,
            string description,
            string state)
        {
            var evt = new Event
            {
                Id = 300,
                Task = EventTasks.Failure,
                Message = $"Something goes wrong in the openid process, code : {code}, description : {description}, state : {state}"
            };

            LogError(evt);
        }

        #endregion
    }
}
