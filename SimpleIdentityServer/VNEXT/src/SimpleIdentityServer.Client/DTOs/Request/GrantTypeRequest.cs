namespace SimpleIdentityServer.Client.DTOs.Request
{
    public enum GrantTypeRequest
    {
        None,
        password,
        client_credentials,
        validate_bearer
    }
}