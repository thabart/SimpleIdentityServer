namespace SimpleIdentityServer.Api.DTOs.Response
{
    public class ErrorResponse
    {
        public string error { get; set; }
        
        public string error_description { get; set; }
        
        public string error_uri { get; set; }
    }
}