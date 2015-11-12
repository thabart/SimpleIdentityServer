namespace SimpleIdentityServer.Api.DTOs.Request
{
    public enum GrantTypeRequest
    {
        None,
        password,
        client_credentials,
        authorization_code,
        validate_bearer
    }
}