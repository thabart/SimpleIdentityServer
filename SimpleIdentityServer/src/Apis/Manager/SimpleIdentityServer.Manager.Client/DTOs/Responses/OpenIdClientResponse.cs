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
    }
}
