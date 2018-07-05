using SimpleIdentityServer.Authenticate.SMS.Common.Requests;
using SimpleIdentityServer.Authenticate.SMS.Common.Responses;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Authenticate.SMS.Client
{
    public interface ISidSmsAuthenticateClient
    {
        Task<ErrorResponse> Send(string requestUrl, ConfirmationCodeRequest request, string authorizationValue = null);
    }

    internal sealed class SidSmsAuthenticateClient : ISidSmsAuthenticateClient
    {
        private readonly ISendSmsOperation _sendSmsOperation;

        public SidSmsAuthenticateClient(ISendSmsOperation sendSmsOperation)
        {
            _sendSmsOperation = sendSmsOperation;
        }

        public Task<ErrorResponse> Send(string requestUrl, ConfirmationCodeRequest request, string authorizationValue = null)
        {
            requestUrl += "/code";
            return _sendSmsOperation.Execute(new Uri(requestUrl), request, authorizationValue);
        }
    }
}
