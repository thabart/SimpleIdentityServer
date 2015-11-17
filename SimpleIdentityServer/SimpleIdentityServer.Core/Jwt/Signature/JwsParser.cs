using SimpleIdentityServer.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.Core.Jwt.Signature
{
    public interface IJwsParser
    {
        JwsPayload ValidateSignature(string jws);
    }

    public class JwsParser
    {
        private readonly IJsonWebKeyRepository _jsonWebKeyRepository;

        private readonly ICreateJwsSignature _createJwsSignature;

        public JwsParser(
            IJsonWebKeyRepository jsonWebKeyRepository,
            ICreateJwsSignature createJwsSignature)
        {
            _jsonWebKeyRepository = jsonWebKeyRepository;
            _createJwsSignature = createJwsSignature;
        }

        /// <summary>
        /// Validate the signature and returns the JWSPayLoad.
        /// </summary>
        /// <param name="jws"></param>
        /// <returns></returns>
        public JwsPayload ValidateSignature(string jws)
        {
            var parts = GetParts(jws);
            if (!parts.Any())
            {
                return null;
            }

            var base64EncodedProtectedHeader = parts[0];
            var base64EncodedSerialized = parts[1];
            var combinedProtectedHeaderAndPayLoad = string.Format("{0}.{1}", base64EncodedProtectedHeader,
                base64EncodedSerialized);

            var serializedProtectedHeader = base64EncodedProtectedHeader.Base64Decode();
            var serializedPayload = base64EncodedSerialized.Base64Decode();
            var signature = parts[2].Base64Decode();

            var protectedHeader = serializedProtectedHeader.DeserializeWithJavascript<JwsProtectedHeader>();
            
            JwsAlg jwsAlg;
            if (!Enum.TryParse(protectedHeader.alg, out jwsAlg))
            {
                // TODO : maybe throw an exception.
                return null;
            }

            var jsonWebKey = _jsonWebKeyRepository.GetByKid(protectedHeader.kid);
            // Checks the JWK exist.
            if (jsonWebKey == null)
            {
                // TODO : throw an exception
                return null;
            }

            var signatureIsCorrect = false;
            switch (jsonWebKey.Kty)
            {
                case KeyType.RSA:
                    // To validate we need the parameters : modulus & exponent.
                    signatureIsCorrect = _createJwsSignature.VerifyWithRsa(
                        jwsAlg, 
                        jsonWebKey.SerializedKey,
                        combinedProtectedHeaderAndPayLoad,
                        signature);
                    break;
            }

            if (!signatureIsCorrect)
            {
                return null;
            }

            return serializedPayload.DeserializeWithJavascript<JwsPayload>();
        }

        /// <summary>
        /// Split the JWS into three parts.
        /// </summary>
        /// <param name="jws"></param>
        /// <returns></returns>
        private static List<string> GetParts(string jws)
        {
            var parts = jws.Split('.');
            return parts.Length < 3 ? new List<string>() : parts.ToList();
        }
    }
}
