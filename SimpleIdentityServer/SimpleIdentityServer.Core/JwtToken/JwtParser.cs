using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.Core.JwtToken
{
    public interface IJwtParser
    {
        string Decrypt(
               string jwe);

        JwsPayload UnSign(
            string jws);
    }

    public class JwtParser : IJwtParser
    {
        private readonly IJweParser _jweParser;

        private readonly IJwsParser _jwsParser;

        private readonly IClientRepository _clientRepository;

        private readonly IJsonWebKeyRepository _jsonWebKeyRepository;

        public JwtParser(
            IJweParser jweParser,
            IJwsParser jwsParser,
            IJsonWebKeyRepository jsonWebKeyRepository)
        {
            _jweParser = jweParser;
            _jwsParser = jwsParser;
            _jsonWebKeyRepository = jsonWebKeyRepository;
        }

        public string Decrypt(
            string jwe)
        {
            var protectedHeader = _jweParser.GetHeader(jwe);
            if (protectedHeader == null)
            {
                return jwe;
            }

            var jsonWebKey = _jsonWebKeyRepository.GetByKid(protectedHeader.Kid);
            if (jsonWebKey == null)
            {
                return jwe;
            }

            return _jweParser.Parse(jwe,
                jsonWebKey);
        }

        public JwsPayload UnSign(
            string jws)
        {
            var protectedHeader = _jwsParser.GetHeader(jws);
            if (protectedHeader == null)
            {
                return null;
            }

            var jsonWebKey = _jsonWebKeyRepository.GetByKid(protectedHeader.Kid);
            return _jwsParser.ValidateSignature(jws, jsonWebKey);
        }
    }
}
