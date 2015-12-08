using Microsoft.Owin.Testing;
using Microsoft.Practices.Unity;

using NUnit.Framework;

using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.Api.Tests.Common.Fakes;
using SimpleIdentityServer.Api.Tests.Common.Fakes.Models;
using SimpleIdentityServer.Api.Tests.Extensions;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.DataAccess.Fake;
using SimpleIdentityServer.DataAccess.Fake.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

using DOMAINS = SimpleIdentityServer.Core.Models;
using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetTokenViaAuthorizationCodeGrantType")]
    public class GetTokenViaAuthorizationCodeGrantTypeSpec
    {
        private HttpResponseMessage _authorizationResponseMessage;

        private DOMAINS.GrantedToken _grantedToken;

        private readonly GlobalContext _context;

        private JwsPayload _jwsPayload;

        private static TestServer _testServer;
        
        private MODELS.ResourceOwner _resourceOwner;

        private JwsProtectedHeader _jwsProtectedHeader;

        private JwsPayload _jwsPayLoad;

        private string _signedPayLoad;

        private string _combinedHeaderAndPayload;

        private Dictionary<string, string> _tokenParameters;

        private string _clientAssertion;

        public GetTokenViaAuthorizationCodeGrantTypeSpec(GlobalContext context)
        {
            context.Init(new FakeGetRateLimitationElementOperation
            {
                Enabled = false
            });

            _context = context;
            _context.UnityContainer.RegisterType<ISimpleIdentityServerConfigurator, SimpleIdentityServerConfigurator>();
        }

        #region Given requests

        [Given("create a resource owner")]
        public void GivenCreateAResourceOwner(Table table)
        {
            _resourceOwner = table.CreateInstance<MODELS.ResourceOwner>();
        }

        [Given("the following address is assigned to the resource owner")]
        public void GivenTheAddressIsAssignedToTheAuthenticatedResourceOwner(Table table)
        {
            var address = table.CreateInstance<MODELS.Address>();
            _resourceOwner.Address = address;
        }

        [Given("authenticate the resource owner")]
        public void GivenAuthenticateTheResourceOwner()
        {
            FakeDataSource.Instance().ResourceOwners.Add(_resourceOwner);
        }
        
        [Given("requesting an authorization code")]
        public void GivenRequestingAnAuthorizationCode(Table table)
        {
            var authorizationRequest = table.CreateInstance<AuthorizationRequest>();
            var httpConfiguration = new HttpConfiguration();
            httpConfiguration.Filters.Add(new FakeAuthenticationFilter
            {
                ResourceOwner = _resourceOwner
            });
            _testServer = _context.CreateServer(httpConfiguration);
            var httpClient = _testServer.HttpClient;
            var url = string.Format(
                "/authorization?scope={0}&response_type={1}&client_id={2}&redirect_uri={3}&prompt={4}&state={5}&nonce={6}",
                authorizationRequest.scope,
                authorizationRequest.response_type,
                authorizationRequest.client_id,
                authorizationRequest.redirect_uri,
                authorizationRequest.prompt,
                authorizationRequest.state,
                authorizationRequest.nonce);
            _authorizationResponseMessage = httpClient.GetAsync(url).Result;
        }

        [Given("create a request to retrieve a token")]
        public void GivenCreateRequestToRetrieveAToken(
            Table table)
        {
            var request = table.CreateInstance<TokenRequest>();
            var query = HttpUtility.ParseQueryString(_authorizationResponseMessage.Headers.Location.Query);
            var authorizationCode = query["code"];
            request.code = authorizationCode;

            _tokenParameters = new Dictionary<string, string>
            {
                {
                    "grant_type",
                    Enum.GetName(typeof (GrantTypeRequest), request.grant_type)
                },
                {
                    "code",
                    authorizationCode
                },
                {
                    "redirect_uri",
                    request.redirect_uri
                },
                {
                    "client_assertion_type",
                    request.client_assertion_type
                }
            };
        }

        [Given("create the JWS payload")]
        public void GivenTheJsonWebTokenIs(Table table)
        {
            var record = table.CreateInstance<FakeJwsPayload>();
            _jwsPayload = record.ToBusiness();
        }

        [Given("assign audiences (.*) to the JWS payload")]
        public void GivenAssignAudiencesToJwsPayload(List<string> audiences)
        {
            _jwsPayload.Add(Core.Jwt.Constants.StandardClaimNames.Audiences, audiences.ToArray());
        }

        [Given("expiration time (.*) in seconds to the JWS payload")]
        public void GivenExpirationTimeInSecondsToJwsPayload(int expirationTimeInSeconds)
        {
            var expiration = DateTime.UtcNow.AddSeconds(expirationTimeInSeconds).ConvertToUnixTimestamp();
            _jwsPayload.Add(Core.Jwt.Constants.StandardClaimNames.ExpirationTime, expiration);
        }

        [Given("sign the jws payload with (.*) kid")]
        public void GivenSignTheJwsPayloadWithKid(string kid)
        {
            var jsonWebKey = FakeDataSource.Instance().JsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            Assert.IsNotNull(jsonWebKey);
            var generator = _context.UnityContainer.Resolve<JwsGenerator>();
            var enumName = Enum.GetName(typeof(AllAlg), jsonWebKey.Alg);
            var alg = (JwsAlg)Enum.Parse(typeof(JwsAlg), enumName);
            _clientAssertion = generator.Generate(_jwsPayload,
                alg,
                jsonWebKey.ToBusiness());
        }

        [Given("encrypt the jws token with (.*) kid, encryption algorithm (.*) and password (.*)")]
        public void GivenEncryptTheJwsPayloadWithKid(string kid, JweEnc enc, string password)
        {
            var jsonWebKey = FakeDataSource.Instance().JsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            Assert.IsNotNull(jsonWebKey);
            var generator = _context.UnityContainer.Resolve<JweGenerator>();
            var algEnumName = Enum.GetName(typeof(AllAlg), jsonWebKey.Alg);
            var alg = (JweAlg)Enum.Parse(typeof(JweAlg), algEnumName);
            _clientAssertion = generator.GenerateJweByUsingSymmetricPassword(
                _clientAssertion,
                alg,
                enc,
                jsonWebKey.ToBusiness(),
                password);
        }

        [Given("set the client assertion value")]
        public void GivenSetClientAssertion()
        {
            _tokenParameters.Add("client_assertion", _clientAssertion);
        }

        [Given("set the client id (.*) into the request")]
        public void GivenSetTheClientIdIntoTheRequest(string clientId)
        {
            _tokenParameters.Add("client_id", clientId);
        }

        #endregion

        #region When requests

        [When("retrieve token via client assertion authentication")]
        public void WhenRetrieveTokenViaClientAssertionAuthentication()
        {
            var httpClient = _testServer.HttpClient;
            httpClient.DefaultRequestHeaders.Clear();
            var response = httpClient.PostAsync("/token", new FormUrlEncodedContent(_tokenParameters)).Result;
            _grantedToken = response.Content.ReadAsAsync<DOMAINS.GrantedToken>().Result;
        }

        [When("requesting a token with basic client authentication for the client id (.*) and client secret (.*)")]
        public void WhenRequestingATokenWithTheAuthorizationCodeFlow(
            string clientId, 
            string clientSecret, 
            Table table)
        {
            var request = table.CreateInstance<TokenRequest>();
            var query = HttpUtility.ParseQueryString(_authorizationResponseMessage.Headers.Location.Query);
            var authorizationCode = query["code"];
            request.code = authorizationCode;

            var dic = new Dictionary<string, string>
            {
                {
                    "grant_type",
                    Enum.GetName(typeof (GrantTypeRequest), request.grant_type)
                },
                {
                    "client_id",
                    request.client_id
                },
                {
                    "code",
                    authorizationCode
                },
                {
                    "redirect_uri",
                    request.redirect_uri
                }
            };

            var httpClient = _testServer.HttpClient;
            var credentials = clientId + ":" + clientSecret;
            httpClient.DefaultRequestHeaders.Add("Authorization", string.Format("Basic {0}", credentials.Base64Encode()));

            var response = httpClient.PostAsync("/token", new FormUrlEncodedContent(dic)).Result;
            _grantedToken = response.Content.ReadAsAsync<DOMAINS.GrantedToken>().Result;
        }

        [When("requesting a token by using a client_secret_post authentication mechanism")]
        public void WhenRequestingATokenByUsingClientSecretPostAuthMech(
            Table table)
        {
            var request = table.CreateInstance<TokenRequest>();
            var query = HttpUtility.ParseQueryString(_authorizationResponseMessage.Headers.Location.Query);
            var authorizationCode = query["code"];
            request.code = authorizationCode;

            var dic = new Dictionary<string, string>
            {
                {
                    "grant_type",
                    Enum.GetName(typeof (GrantTypeRequest), request.grant_type)
                },
                {
                    "client_id",
                    request.client_id
                },
                {
                    "client_secret",
                    request.client_secret
                },
                {
                    "code",
                    authorizationCode
                },
                {
                    "redirect_uri",
                    request.redirect_uri
                }
            };

            var httpClient = _testServer.HttpClient;
            httpClient.DefaultRequestHeaders.Clear();
            var response = httpClient.PostAsync("/token", new FormUrlEncodedContent(dic)).Result;
            _grantedToken = response.Content.ReadAsAsync<DOMAINS.GrantedToken>().Result;
        }

        #endregion

        #region Then requests

        [Then("the following token is returned")]
        public void ThenTheCallbackContainsTheQueryName(Table table)
        {
            var record = table.CreateInstance<DOMAINS.GrantedToken>();

            Assert.That(record.TokenType, Is.EqualTo(_grantedToken.TokenType));
        }

        [Then("decrypt the id_token parameter from the response")]
        public void ThenDecryptTheIdTokenFromTheQueryString()
        {
            Assert.IsNotNull(_grantedToken.IdToken);

            var parts = _grantedToken.IdToken.Split('.');

            Assert.That(parts.Length >= 3, Is.True);

            var secondPart = parts[1].Base64Decode();

            var javascriptSerializer = new JavaScriptSerializer();
            _jwsProtectedHeader = javascriptSerializer.Deserialize<JwsProtectedHeader>(parts[0].Base64Decode());
            _jwsPayLoad = javascriptSerializer.Deserialize<JwsPayload>(secondPart);
            _combinedHeaderAndPayload = parts[0] + "." + parts[1];
            _signedPayLoad = parts[2];
        }

        [Then("the signature of the JWS payload is valid")]
        public void ThenTheSignatureIsCorrect()
        {
            using (var provider = new RSACryptoServiceProvider())
            {
                var serializedRsa = FakeDataSource.Instance().JsonWebKeys.First().SerializedKey;
                provider.FromXmlString(serializedRsa);
                var signature = _signedPayLoad.Base64DecodeBytes();
                var payLoad = ASCIIEncoding.ASCII.GetBytes(_combinedHeaderAndPayload);
                var signatureIsCorrect = provider.VerifyData(payLoad, "SHA256", signature);
                Assert.IsTrue(signatureIsCorrect);
            }
        }

        [Then("the protected JWS header is returned")]
        public void ThenProtectedJwsHeaderIsReturned(Table table)
        {
            var record = table.CreateInstance<JwsProtectedHeader>();

            Assert.That(record.Alg, Is.EqualTo(_jwsProtectedHeader.Alg));
        }

        [Then("the parameter nonce with value (.*) is returned by the JWS payload")]
        public void ThenNonceIsReturnedInJwsPayLoad(string nonce)
        {
            Assert.That(_jwsPayLoad.Nonce, Is.EqualTo(nonce));
        }

        [Then("the claim (.*) with value (.*) is returned by the JWS payload")]
        public void ThenTheClaimWithValueIsReturnedByJwsPayLoad(string claimName, string val)
        {
            var claimValue = _jwsPayLoad.GetClaimValue(claimName);

            Assert.IsNotNull(claimValue);
            Assert.That(claimValue, Is.EqualTo(val));
        }

        #endregion

        [AfterScenario]
        public static void After()
        {
            _testServer.Dispose();
        }
    }
}
