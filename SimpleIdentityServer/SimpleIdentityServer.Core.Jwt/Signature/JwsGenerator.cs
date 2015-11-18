using System;
using SimpleIdentityServer.Core.Common.Extensions;

namespace SimpleIdentityServer.Core.Jwt.Signature
{
    public interface IJwsGenerator
    {
        string Generate(
            JwsPayload payload,
            JwsAlg jwsAlg,
            JsonWebKey jsonWebKey);
    }

    public class JwsGenerator : IJwsGenerator
    {
        private const string JwsType = "JWT";

        private readonly ICreateJwsSignature _createJwsSignature;

        public JwsGenerator(
            ICreateJwsSignature createJwsSignature)
        {
            _createJwsSignature = createJwsSignature;
        }
        
        public string Generate(
            JwsPayload payload,
            JwsAlg jwsAlg,
            JsonWebKey jsonWebKey)
        {
            var protectedHeader = ConstructProtectedHeader(jwsAlg);

            if (jsonWebKey != null &&
                jwsAlg != JwsAlg.none)
            {
                protectedHeader.Kid = jsonWebKey.Kid;
            }

            var serializedProtectedHeader = protectedHeader.SerializeWithDataContract();
            var base64EncodedSerializedProtectedHeader = serializedProtectedHeader.Base64Encode();
            var serializedPayload = payload.SerializeWithJavascript();
            var base64EncodedSerializedPayload = serializedPayload.Base64Encode();
            var combinedProtectedHeaderAndPayLoad = string.Format(
                "{0}.{1}", 
                base64EncodedSerializedProtectedHeader, 
                base64EncodedSerializedPayload);

            var signedJws = string.Empty;
            if (jsonWebKey != null)
            {
                switch (jsonWebKey.Kty)
                {
                    case KeyType.RSA:
                        signedJws = _createJwsSignature.SignWithRsa(jwsAlg, jsonWebKey.SerializedKey, combinedProtectedHeaderAndPayLoad);
                        break;
                }
            }

            return string.Format("{0}.{1}", combinedProtectedHeaderAndPayLoad, signedJws);
        }

        private JwsProtectedHeader ConstructProtectedHeader(JwsAlg alg)
        {
            return new JwsProtectedHeader
            {
                Alg = Enum.GetName(typeof(JwsAlg), alg),
                Type = JwsType
            };
        }
    }
}
