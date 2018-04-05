namespace SimpleIdentityServer.Manager.Client.DTOs.Responses
{
    public class BaseResponse
    {
        public bool ContainsError { get; set; }
        public ErrorResponse Error { get; set; }
    }
}
