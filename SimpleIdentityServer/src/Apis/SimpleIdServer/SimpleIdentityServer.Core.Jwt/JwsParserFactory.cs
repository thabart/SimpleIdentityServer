using SimpleIdentityServer.Core.Jwt.Signature;

namespace SimpleIdentityServer.Core.Jwt
{
    public class JwsParserFactory
    {
        public IJwsParser BuildJwsParser()
        {
            var createJwsSignature = new CreateJwsSignature();
            return new JwsParser(createJwsSignature);
        }
    }
}
