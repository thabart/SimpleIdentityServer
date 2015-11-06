using System.Web.Script.Serialization;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.Core.IdToken.Generator
{
    public interface IJwsGenerator
    {
        
    }

    /// <summary>
    /// Generate a JSON Web Signature
    /// </summary>
    public class JwsGenerator
    {
        private readonly IClientRepository _clientRepository;

        public JwsGenerator(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public string GenerateJws(
            JwsProtectedHeader jwsProtectedHeader,
            JwtClaims jwtClaims,
            AuthorizationParameter authorizationParameter)
        {
            // https://tools.ietf.org/html/draft-ietf-jose-json-web-signature-41#appendix-A.1
            var javascriptSerializer = new JavaScriptSerializer();
            var jsonJwsProtectedHeader = javascriptSerializer.Serialize(jwsProtectedHeader);
            var jwsProtectedHeaderBase64Encoded = jsonJwsProtectedHeader.Base64Encode();
            var jwsPayLoad = javascriptSerializer.Serialize(jwtClaims);
            var jwsPayLoadBase64Encoded = jwsPayLoad.Base64Encode();

            var combined = string.Format("{0}.{1}", jwsProtectedHeaderBase64Encoded, jwsPayLoadBase64Encoded);

            // Based on the client settings we'll encrypt & signed the id_token in different ways.
            // In the "open-id-connect-discovery" there's an endpoint jwks_uri :
            // This url contains the signing key's) the RP uses to validate signatures from the OP
            // The JWS set may also contain the Server's encryption key(s) which are used by the RP to encrypt requests to the server.

            var client = _clientRepository.GetClientById(authorizationParameter.ClientId);



            return string.Empty;
        }
    }
}
