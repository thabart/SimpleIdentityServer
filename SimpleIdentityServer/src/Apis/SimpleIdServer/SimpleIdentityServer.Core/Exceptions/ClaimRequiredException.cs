namespace SimpleIdentityServer.Core.Exceptions
{
    public class ClaimRequiredException : IdentityServerException
    {
        public ClaimRequiredException(string claim)
        {
            Claim = claim;
        }

        public string Claim { get; private set; }
    }
}
