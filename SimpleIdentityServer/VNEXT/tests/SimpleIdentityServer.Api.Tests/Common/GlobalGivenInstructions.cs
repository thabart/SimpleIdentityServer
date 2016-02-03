using SimpleIdentityServer.Api.Tests.Common.Fakes;
using SimpleIdentityServer.Api.Tests.Common.Fakes.Models;
using SimpleIdentityServer.Api.Tests.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.DataAccess.Fake;
using SimpleIdentityServer.DataAccess.Fake.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace SimpleIdentityServer.Api.Tests.Common
{
    [Binding]
    public class GlobalGivenInstructions
    {
        private readonly ISecurityHelper _securityHelper;
        
        private GlobalContext _globalContext;
        
        public GlobalGivenInstructions(GlobalContext globalContext)
        {
            _securityHelper = new SecurityHelper();
            _globalContext = globalContext;
        }

        [BeforeScenario]
        public void Before()
        {            
            _globalContext.Init();
        }
        
        [Given("a mobile application (.*) is defined")]
        public void GivenClient(string clientId)
        {
            var client = new Client
            {
                ClientId = clientId,
                AllowedScopes = new List<Scope>()
            };

            _globalContext.FakeDataSource.Clients.Add(client);
        }

        [Given("a resource owner with username (.*) and password (.*) is defined")]
        public void GivenResourceOwner(string userName, string password)
        {
            var resourceOwner = new ResourceOwner
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
                    _globalContext.FakeDataSource.JsonWebKeys.Add(new JsonWebKey
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
        public void GivenTokenEndPointAuthenticationMethodIsAssigned(TokenEndPointAuthenticationMethods tokenEndPtAuthMethod, string clientId)
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
        public void GivenGrantTypesAreSupportedByClient(GrantType grantType, string clientId)
        {
            var client = GetClient(clientId);
            if (client == null)
            {
                return;
            }

            client.GrantTypes = new List<GrantType>
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

            client.ResponseTypes = new List<ResponseType>();
            foreach (var responseType in responseTypes)
            {
                var resp = (ResponseType)Enum.Parse(typeof(ResponseType), responseType);
                client.ResponseTypes.Add(resp);
            }
        }

        [Given("the consent has been given by the resource owner (.*) for the client (.*) and scopes (.*)")]
        public void GivenConsentScopes(string resourceOwnerId, string clientId, List<string> scopeNames)
        {
            var client = _globalContext.FakeDataSource.Clients.SingleOrDefault(c => c.ClientId == clientId);
            var resourceOwner = _globalContext.FakeDataSource.ResourceOwners.SingleOrDefault(r => r.Id == resourceOwnerId);
            var scopes = new List<Scope>();
            foreach (var scopeName in scopeNames)
            {
                var storedScope = _globalContext.FakeDataSource.Scopes.SingleOrDefault(s => s.Name == scopeName);
                scopes.Add(storedScope);
            }
            var consent = new Consent
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
            var consent = new Consent
            {
                Client = client,
                Claims = claimNames,
                ResourceOwner = resourceOwner
            };

            _globalContext.FakeDataSource.Consents.Add(consent);
        }

        private Client GetClient(string clientId)
        {
            var client = _globalContext.FakeDataSource.Clients.SingleOrDefault(c => c.ClientId == clientId);
            return client;
        }
    }
}
