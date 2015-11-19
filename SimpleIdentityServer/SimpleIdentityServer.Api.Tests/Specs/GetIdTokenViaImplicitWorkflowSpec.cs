using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;

using Microsoft.Practices.Unity;

using NUnit.Framework;

using SimpleIdentityServer.Api.DTOs.Request;
using SimpleIdentityServer.Api.Tests.Common;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.DataAccess.Fake;
using SimpleIdentityServer.DataAccess.Fake.Extensions;
using SimpleIdentityServer.RateLimitation.Configuration;

using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;

using System.Web;
using SimpleIdentityServer.Core.Extensions;
using System.Web.Script.Serialization;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Api.Tests.Common.Fakes.Models;
using SimpleIdentityServer.Api.Tests.Common.Fakes;

namespace SimpleIdentityServer.Api.Tests.Specs
{
    [Binding, Scope(Feature = "GetIdTokenViaImplicitWorkflow")]
    public sealed class GetIdTokenViaImplicitWorkflowSpec
    {
        private readonly GlobalContext _context;

        private HttpResponseMessage _httpResponseMessage;

        private FakeUserInformation _fakeUserInformation;

        private JwsProtectedHeader _jwsProtectedHeader;

        private string _jws;

        private JwsPayload _jwsPayLoad;

        private string _signedPayLoad;

        private string _combinedHeaderAndPayload;

        public GetIdTokenViaImplicitWorkflowSpec(GlobalContext context)
        {
            var fakeGetRateLimitationElementOperation = new FakeGetRateLimitationElementOperation
            {
                Enabled = false
            };
            
            _context = context;
            _context.UnityContainer.RegisterInstance<IGetRateLimitationElementOperation>(fakeGetRateLimitationElementOperation);
        }

        [Given("a resource owner is authenticated")]
        public void GivenAResourceOwnerIsAuthenticated(Table table)
        {
            _fakeUserInformation = table.CreateInstance<FakeUserInformation>();
            var resourceOwner = new MODELS.ResourceOwner
            {
                Id = _fakeUserInformation.UserId
            };
            FakeDataSource.Instance().ResourceOwners.Add(resourceOwner);
        }

        [When("requesting an authorization")]
        public void WhenRequestingAnAuthorizationCode(Table table)
        {
            var authorizationRequest = table.CreateInstance<AuthorizationRequest>();
            // Fake the authentication filter.
            var httpConfiguration = new HttpConfiguration();
            if (_fakeUserInformation != null)
            {
                httpConfiguration.Filters.Add(new FakeAuthenticationFilter
                {
                    ResourceOwnerId = _fakeUserInformation.UserId,
                    ResourceOwnerUserName = _fakeUserInformation.UserName
                });
            }

            using (var server = _context.CreateServer(httpConfiguration))
            {
                var httpClient = server.HttpClient;
                var url = string.Format(
                    "/authorization?scope={0}&response_type={1}&client_id={2}&redirect_uri={3}&prompt={4}&state={5}&nonce={6}",
                    authorizationRequest.scope,
                    authorizationRequest.response_type,
                    authorizationRequest.client_id,
                    authorizationRequest.redirect_uri,
                    authorizationRequest.prompt,
                    authorizationRequest.state,
                    authorizationRequest.nonce);
                _httpResponseMessage = httpClient.GetAsync(url).Result;
            }
        }
        
        [Then("the http status code is (.*)")]
        public void ThenHttpStatusCodeIsCorrect(HttpStatusCode code)
        {
            Assert.That(code, Is.EqualTo(_httpResponseMessage.StatusCode));
        }

        [Then("redirect to (.*) controller")]
        public void ThenRedirectToController(string controller)
        {
            var location = _httpResponseMessage.Headers.Location;
            Assert.That(location.AbsolutePath, Is.EqualTo(controller));
        }

        [Then("decrypt the id_token parameter from the query string")]
        public void ThenDecryptTheIdTokenFromTheQueryString()
        {
            var location = _httpResponseMessage.Headers.Location;
            var query = HttpUtility.ParseQueryString(location.Query);
            var idToken = query["id_token"];

            Assert.IsNotNull(idToken);

            var parts = idToken.Split('.');

            Assert.That(parts.Count() >= 3, Is.True);
            _jws = idToken;

            var secondPart = parts[1].Base64Decode();

            var javascriptSerializer = new JavaScriptSerializer();
            _jwsProtectedHeader = javascriptSerializer.Deserialize<JwsProtectedHeader>(parts[0].Base64Decode());
            _jwsPayLoad = javascriptSerializer.Deserialize<JwsPayload>(secondPart);
            _combinedHeaderAndPayload = parts[0] + "." + parts[1];
            _signedPayLoad = parts[2];
        }

        [Then("decrypt the jwe parameter from the query string with the following kid (.*)")]
        public void ThenDecryptTheJweParameterFromTheQueryString(string kid)
        {
            var location = _httpResponseMessage.Headers.Location;
            var query = HttpUtility.ParseQueryString(location.Query);
            var idToken = query["id_token"];

            Assert.IsNotNull(idToken);

            var jsonWebKey = FakeDataSource.Instance().JsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            Assert.IsNotNull(jsonWebKey);

            var jweParser = _context.UnityContainer.Resolve<IJweParser>();
            var result = jweParser.Parse(idToken, jsonWebKey.ToBusiness());
            _jws = result;
            var parts = result.Split('.');

            Assert.That(parts.Count() >= 3, Is.True);

            var secondPart = parts[1].Base64Decode();

            var javascriptSerializer = new JavaScriptSerializer();
            _jwsProtectedHeader = javascriptSerializer.Deserialize<JwsProtectedHeader>(parts[0].Base64Decode());
            _jwsPayLoad = javascriptSerializer.Deserialize<JwsPayload>(secondPart);
            _combinedHeaderAndPayload = parts[0] + "." + parts[1];
            _signedPayLoad = parts[2];
        }

        [Then("check the signature is correct with the kid (.*)")]
        public void ThenCheckSignatureIsCorrectWithKid(string kid)
        {
            var jsonWebKey = FakeDataSource.Instance().JsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            Assert.IsNotNull(jsonWebKey);

            var jwsParser = _context.UnityContainer.Resolve<IJwsParser>();
            var result = jwsParser.ValidateSignature(_jws, jsonWebKey.ToBusiness());

            Assert.IsNotNull(result);
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

            Assert.That(record.Alg, Is.EqualTo(_jwsProtectedHeader.Alg));
        }

        [Then("the audience parameter with value (.*) is returned by the JWS payload")]
        public void ThenAudienceIsReturnedInJwsPayLoad(string audience)
        {
            Assert.That(_jwsPayLoad.Audiences.Contains(audience), Is.True);
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

        [Then("the callback contains the following query name (.*)")]
        public void ThenTheCallbackContainsTheQueryName(string queryName)
        {
            var location = _httpResponseMessage.Headers.Location;
            var query = HttpUtility.ParseQueryString(location.Query);
            var queryValue = query[queryName];
            Assert.IsNotNullOrEmpty(queryValue);
        }

        private static MODELS.Client GetClient(string clientId)
        {
            var client = FakeDataSource.Instance().Clients.SingleOrDefault(c => c.ClientId == clientId);
            return client;
        }
    }
}
