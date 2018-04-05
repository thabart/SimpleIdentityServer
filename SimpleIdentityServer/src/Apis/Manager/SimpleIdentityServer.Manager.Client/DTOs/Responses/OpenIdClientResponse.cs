using Newtonsoft.Json.Linq;
using System;

namespace SimpleIdentityServer.Manager.Client.DTOs.Responses
{
    public sealed class OpenIdClientResponse : BaseResponse
    {
        public OpenIdClientResponse()
        {

        }

        public OpenIdClientResponse(ErrorResponse error)
        {
            Error = error;
            ContainsError = true;
        }

        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string LogoUri { get; set; }

        public static OpenIdClientResponse ToClient(JObject jObj)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            return new OpenIdClientResponse
            {
                ClientId = jObj.GetValue(Constants.GetClientsResponseNames.ClientId).ToString(),
                ClientName = jObj.GetValue(Constants.GetClientsResponseNames.ClientName).ToString(),
                LogoUri = jObj.GetValue(Constants.GetClientsResponseNames.LogoUri).ToString()
            };
        }
    }
}
