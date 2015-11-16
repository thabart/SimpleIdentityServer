using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.DataAccess.Fake;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

using SimpleIdentityServer.Api.Tests.Common.Fakes;
using SimpleIdentityServer.Api.Tests.Common.Fakes.Models;

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

        private static TestServer _testServer;

        private FakeUserInformation _fakeUserInfo;

        private JwsProtectedHeader _jwsProtectedHeader;

        private JwsPayload _jwsPayLoad;

        private string _signedPayLoad;

        private string _combinedHeaderAndPayload;

        public GetTokenViaAuthorizationCodeGrantTypeSpec(GlobalContext context)
        {
            context.Init(new FakeGetRateLimitationElementOperation
            {
                Enabled = false
            });

            _context = context;
        }

        [Given("a resource owner is authenticated")]
        public void GivenAResourceOwnerIsAuthenticated(Table table)
        {
            _fakeUserInfo = table.CreateInstance<FakeUserInformation>();
            var resourceOwner = new MODELS.ResourceOwner
            {
                Id = _fakeUserInfo.UserId
            };

            FakeDataSource.Instance().ResourceOwners.Add(resourceOwner);
        }
        
        [Given("requesting an authorization code")]
        public void GivenRequestingAnAuthorizationCode(Table table)
        {
            var authorizationRequest = table.CreateInstance<AuthorizationRequest>();
            var httpConfiguration = new HttpConfiguration();
            httpConfiguration.Filters.Add(new FakeAuthenticationFilter
            {
                ResourceOwnerId = _fakeUserInfo.UserId,
                ResourceOwnerUserName = _fakeUserInfo.UserName
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
                _signedPayLoad = _signedPayLoad.Replace(" ", "+");
                var signature = Convert.FromBase64String(_signedPayLoad);
                var payLoad = ASCIIEncoding.ASCII.GetBytes(_combinedHeaderAndPayload);
                var signatureIsCorrect = provider.VerifyData(payLoad, "SHA256", signature);
                Assert.IsTrue(signatureIsCorrect);
            }
        }

        [Then("the protected JWS header is returned")]
        public void ThenProtectedJwsHeaderIsReturned(Table table)
        {
            var record = table.CreateInstance<JwsProtectedHeader>();

            Assert.That(record.alg, Is.EqualTo(_jwsProtectedHeader.alg));
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
        
        [AfterScenario]
        public static void After()
        {
            _testServer.Dispose();
        }
    }
}
