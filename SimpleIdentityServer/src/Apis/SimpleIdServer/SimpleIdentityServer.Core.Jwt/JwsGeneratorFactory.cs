using SimpleIdentityServer.Core.Jwt.Serializer;
using SimpleIdentityServer.Core.Jwt.Signature;

namespace SimpleIdentityServer.Core.Jwt
{
    public class JwsGeneratorFactory
    {
        public IJwsGenerator BuildJwsGenerator()
        {
            ICreateJwsSignature createJwsSignature;
#if NET461
            createJwsSignature = new CreateJwsSignature(new CngKeySerializer());
#else
            createJwsSignature = new CreateJwsSignature();
#endif
            return new JwsGenerator(createJwsSignature);
        }
    }
}