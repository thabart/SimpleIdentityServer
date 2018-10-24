using SimpleIdentityServer.Authenticate.SMS.Common.Requests;
using SimpleIdentityServer.Common.Client;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Authenticate.SMS.Client
{
    public interface ISidSmsAuthenticateClient
    {
        Task<BaseResponse> Send(string requestUrl, ConfirmationCodeRequest request, string authorizationValue = null);
    }

    internal sealed class SidSmsAuthenticateClient : ISidSmsAuthenticateClient
    {
        private readonly ISendSmsOperation _sendSmsOperation;

        public SidSmsAuthenticateClient(ISendSmsOperation sendSmsOperation)
        {
            _sendSmsOperation = sendSmsOperation;
        }

        public Task<BaseResponse> Send(string requestUrl, ConfirmationCodeRequest request, string authorizationValue = null)
        {
            requestUrl += "/code";
            return _sendSmsOperation.Execute(new Uri(requestUrl), request, authorizationValue);
        }
    }
}
