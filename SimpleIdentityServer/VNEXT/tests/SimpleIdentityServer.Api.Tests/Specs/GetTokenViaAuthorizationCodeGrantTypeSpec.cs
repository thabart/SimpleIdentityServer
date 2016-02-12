using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.DataAccess.Fake.Extensions;
using SimpleIdentityServer.Host.DTOs.Request;
using SimpleIdentityServer.RateLimitation.Configuration;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Xunit;
using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.Api.Tests.Common.Fakes;
using SimpleIdentityServer.Api.Tests.Common.Fakes.Models;
using SimpleIdentityServer.Api.Tests.Extensions;
using DOMAINS = SimpleIdentityServer.Core.Models;
using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;
using SimpleIdentityServer.Core.Helpers;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetTokenViaAuthorizationCodeGrantType")]
    public class GetTokenViaAuthorizationCodeGrantTypeSpec
    {
        private HttpResponseMessage _authorizationResponseMessage;
        
        private ISecurityHelper _securityHelper;

        private DOMAINS.GrantedToken _grantedToken;

        private GlobalContext _globalContext;

        private JwsPayload _jwsPayload;
        
        private MODELS.ResourceOwner _resourceOwner;

        private JwsProtectedHeader _jwsProtectedHeader;

        private JwsPayload _jwsPayLoad;

        private string _signedPayLoad;

        private string _combinedHeaderAndPayload;

        private Dictionary<string, string> _tokenParameters;

        private string _clientAssertion;

        #region Given requests

        [BeforeScenario]
        private void Init() 
        {            
            _securityHelper = new SecurityHelper();
            _globalContext = new GlobalContext();
            _globalContext.Init();
            _globalContext.CreateServer(services => {
                var fakeGetRateLimitationElementOperation = new FakeGetRateLimitationElementOperation
                {
                    Enabled = false
                };
                services.AddInstance<IGetRateLimitationElementOperation>(fakeGetRateLimitationElementOperation);
                services.AddTransient<ISimpleIdentityServerConfigurator, SimpleIdentityServerConfigurator>();
            });
        }

        
        [Given("a mobile application (.*) is defined")]
        public void GivenClient(string clientId)
        {
            var client = new MODELS.Client
            {
                ClientId = clientId,
                AllowedScopes = new List<MODELS.Scope>()
            };

            _globalContext.FakeDataSource.Clients.Add(client);
        }

        [Given("a resource owner with username (.*) and password (.*) is defined")]
        public void GivenResourceOwner(string userName, string password)
        {
            var resourceOwner = new MODELS.ResourceOwner
            {
                Name = userName,
                Password = _securityHelper.ComputeHash(password)
            };

            _globalContext.FakeDataSource.ResourceOwners.Add(resourceOwner);
        }

        [Given("add json web keys")]
        public void AddJsonWebKeys(Table table)
        {
            var jsonWebKeys = table.CreateSet<FakeJsonWebKey>();
            using (var provider = new RSACryptoServiceProvider())
            {
                foreach (var jsonWebKey in jsonWebKeys)
                {
                    var serializedRsa = provider.ToXmlString(true);
                    _globalContext.FakeDataSource.JsonWebKeys.Add(new MODELS.JsonWebKey
                    {
                        Alg = jsonWebKey.Alg,
                        KeyOps = new[]
                        {
                        jsonWebKey.Operation
                    },
                        Kid = jsonWebKey.Kid,
                        Kty = jsonWebKey.Kty,
                        Use = jsonWebKey.Use,
                        SerializedKey = serializedRsa
                    });
                }
            }
        }

        [Given("the scopes are defined")]
        public void GivenScope(Table table)
        {
            var scopes = table.CreateSet<FakeScope>();
            foreach (var scope in scopes)
            {
                _globalContext.FakeDataSource.Scopes.Add(scope.ToFake());
            }
        }
        
        [Given("the scopes (.*) are assigned to the client (.*)")]
        public void GivenScopesToTheClients(List<string> scopeNames, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            var scopes = _globalContext.FakeDataSource.Scopes;
            foreach (var scopeName in scopeNames)
            {
                var storedScope = scopes.SingleOrDefault(s => s.Name == scopeName);
                if (storedScope == null)
                {
                    continue;
                }

                client.AllowedScopes.Add(storedScope);
            }
        }
        
        [Given("the token endpoint authentication method (.*) is assigned to the client (.*)")]
        public void GivenTokenEndPointAuthenticationMethodIsAssigned(MODELS.TokenEndPointAuthenticationMethods tokenEndPtAuthMethod, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            client.TokenEndPointAuthMethod = tokenEndPtAuthMethod;
        }

        [Given("the redirection uri (.*) is assigned to the client (.*)")]
        public void GivenRedirectionUriIsAssignedTo(string redirectionUri, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            client.RedirectionUrls = new List<string>
            {
                redirectionUri
            };
        }

        [Given("the client secret (.*) is assigned to the client (.*)")]
        public void GivenScopesToTheClient(string clientSecret, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            client.ClientSecret = clientSecret;
        }

        [Given("the id_token encrypted response alg is set to (.*) for the client (.*)")]
        public void GivenEncryptedResponseAlgToTheClient(string algorithm, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            client.IdTokenEncryptedResponseAlg = algorithm;
        }

        [Given("the id_token encrypted response enc is set to (.*) for the client (.*)")]
        public void GivenEncryptedResponseEncToTheClient(string enc, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            client.IdTokenEncryptedResponseEnc = enc;
        }

        [Given("the id_token signature algorithm is set to (.*) for the client (.*)")]
        public void GivenIdTokenSignatureAlgorithmIsSetForTheClient(string algorithm, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            client.IdTokenSignedResponseAlg = algorithm;
        }

        [Given("the grant-type (.*) is supported by the client (.*)")]
        public void GivenGrantTypesAreSupportedByClient(MODELS.GrantType grantType, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            client.GrantTypes = new List<MODELS.GrantType>
            {
                grantType
            };
        }

        [Given("the response-types (.*) are supported by the client (.*)")]
        public void GivenResponseIsSupportedByTheClient(List<string> responseTypes, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            client.ResponseTypes = new List<MODELS.ResponseType>();
            foreach (var responseType in responseTypes)
            {
                var resp = (MODELS.ResponseType)Enum.Parse(typeof(ResponseType), responseType);
                client.ResponseTypes.Add(resp);
            }
        }

        [Given("the consent has been given by the resource owner (.*) for the client (.*) and scopes (.*)")]
        public void GivenConsentScopes(string resourceOwnerId, string clientId, List<string> scopeNames)
        {
            var client = _globalContext.FakeDataSource.Clients.SingleOrDefault(c => c.ClientId == clientId);
            var resourceOwner = _globalContext.FakeDataSource.ResourceOwners.SingleOrDefault(r => r.Id == resourceOwnerId);
            var scopes = new List<MODELS.Scope>();
            foreach (var scopeName in scopeNames)
            {
                var storedScope = _globalContext.FakeDataSource.Scopes.SingleOrDefault(s => s.Name == scopeName);
                scopes.Add(storedScope);
            }
            var consent = new MODELS.Consent
            {
                Client = client,
                GrantedScopes = scopes,
                ResourceOwner = resourceOwner
            };

            _globalContext.FakeDataSource.Consents.Add(consent);
        }

        [Given("the consent has been given by the resource owner (.*) for the client (.*) and claims (.*)")]
        public void GivenConsentClaims(string resourceOwnerId, string clientId, List<string> claimNames)
        {
            var client = _globalContext.FakeDataSource.Clients.SingleOrDefault(c => c.ClientId == clientId);
            var resourceOwner = _globalContext.FakeDataSource.ResourceOwners.SingleOrDefault(r => r.Id == resourceOwnerId);
            var consent = new MODELS.Consent
            {
                Client = client,
                Claims = claimNames,
                ResourceOwner = resourceOwner
            };

            _globalContext.FakeDataSource.Consents.Add(consent);
        }

        private MODELS.Client GetClient(string clientId)
        {
            var client = _globalContext.FakeDataSource.Clients.SingleOrDefault(c => c.ClientId == clientId);
            return client;
        }

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
            _globalContext.FakeDataSource.ResourceOwners.Add(_resourceOwner);
            _globalContext.AuthenticationMiddleWareOptions.IsEnabled = true;
            _globalContext.AuthenticationMiddleWareOptions.ResourceOwner = _resourceOwner;
        }
        
        [Given("requesting an authorization code")]
        public void GivenRequestingAnAuthorizationCode(Table table)
        {
            var authorizationRequest = table.CreateInstance<AuthorizationRequest>();
            var httpClient = _globalContext.TestServer.CreateClient();
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
            var query = HttpUtility.ParseQueryString(_authorizationResponseMessage.Headers.Location.Query.TrimStart('#'));
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

        [Given("add json web keys (.*) to the client (.*)")]
        public void GivenAddJsonWeKeyToTheClient(List<string> kids, string clientId)
        {
            var client = _globalContext.FakeDataSource.Clients.FirstOrDefault(c => c.ClientId == clientId);
            client.JsonWebKeys = new List<MODELS.JsonWebKey>();
            var jsonWebKeys = _globalContext.FakeDataSource.JsonWebKeys;
            foreach (var kid in kids)
            {
                var jsonWebKey = jsonWebKeys.FirstOrDefault(j => j.Kid.Equals(kid));
                client.JsonWebKeys.Add(jsonWebKey);
            }
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
            var jsonWebKey = _globalContext.FakeDataSource.JsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            Assert.NotNull(jsonWebKey);
            var generator = _globalContext.ServiceProvider.GetService<IJwsGenerator>();
            var enumName = Enum.GetName(typeof(AllAlg), jsonWebKey.Alg);
            var alg = (JwsAlg)Enum.Parse(typeof(JwsAlg), enumName);
            _clientAssertion = generator.Generate(_jwsPayload,
                alg,
                jsonWebKey.ToBusiness());
        }

        [Given("encrypt the jws token with (.*) kid, encryption algorithm (.*) and password (.*)")]
        public void GivenEncryptTheJwsPayloadWithKid(string kid, JweEnc enc, string password)
        {
            var jsonWebKey = _globalContext.FakeDataSource.JsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            Assert.NotNull(jsonWebKey);
            var generator = _globalContext.ServiceProvider.GetService<IJweGenerator>();
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
            var httpClient = _globalContext.TestServer.CreateClient();
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

            var httpClient = _globalContext.TestServer.CreateClient();
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

            var httpClient = _globalContext.TestServer.CreateClient();
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

            Assert.True(record.TokenType.Equals(_grantedToken.TokenType));
        }

        [Then("decrypt the id_token parameter from the response")]
        public void ThenDecryptTheIdTokenFromTheQueryString()
        {
            Assert.NotNull(_grantedToken.IdToken);

            var parts = _grantedToken.IdToken.Split('.');

            Assert.True(parts.Length >= 3);

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
                var serializedRsa = _globalContext.FakeDataSource.JsonWebKeys.First().SerializedKey;
                provider.FromXmlString(serializedRsa);
                var signature = _signedPayLoad.Base64DecodeBytes();
                var payLoad = ASCIIEncoding.ASCII.GetBytes(_combinedHeaderAndPayload);
                var signatureIsCorrect = provider.VerifyData(payLoad, "SHA256", signature);
                Assert.True(signatureIsCorrect);
            }
        }

        [Then("the protected JWS header is returned")]
        public void ThenProtectedJwsHeaderIsReturned(Table table)
        {
            var record = table.CreateInstance<JwsProtectedHeader>();

            Assert.True(record.Alg.Equals(_jwsProtectedHeader.Alg));
        }

        [Then("the parameter nonce with value (.*) is returned by the JWS payload")]
        public void ThenNonceIsReturnedInJwsPayLoad(string nonce)
        {
            Assert.True(_jwsPayLoad.Nonce.Equals(nonce));
        }

        [Then("the claim (.*) with value (.*) is returned by the JWS payload")]
        public void ThenTheClaimWithValueIsReturnedByJwsPayLoad(string claimName, string val)
        {
            var claimValue = _jwsPayLoad.GetClaimValue(claimName);

            Assert.NotNull(claimValue);
            Assert.True(claimValue.Equals(val));
        }

        #endregion

        [AfterScenario]
        public void After()
        {            
            _globalContext.TestServer.Dispose();
        }
    }
}
