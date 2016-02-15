namespace SimpleIdentityServer.Api.DTOs.Response
{
    public class ErrorResponseWithState : ErrorResponse
    {
        public string state { get; set; }
    }
}