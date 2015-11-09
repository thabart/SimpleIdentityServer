namespace SimpleIdentityServer.Core.Jwt.Signature
{
    public class JsonWebKeySet
    {
        /// <summary>
        /// Gets or sets the array of JWK values.
        /// </summary>
        public JsonWebKey[] Keys { get;  set; }
    }
}
