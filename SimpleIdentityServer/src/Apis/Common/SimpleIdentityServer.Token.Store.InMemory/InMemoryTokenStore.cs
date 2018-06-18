using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.DTOs.Response;

namespace SimpleIdentityServer.Token.Store.InMemory
{
    internal sealed class InMemoryTokenStore : ITokenStore
    {
        private class StoredToken
        {
            public StoredToken()
            {
                Scopes = new List<string>();
            }

            public string Url { get; set; }
            public IEnumerable<string> Scopes { get; set; }
            public GrantedToken GrantedToken { get; set; }
            public DateTime ExpirationDateTime { get; set; }
        }

        private readonly List<StoredToken> _tokens;
        private readonly IIdentityServerClientFactory _identityServerClientFactory;

        public InMemoryTokenStore(IIdentityServerClientFactory identityServerClientFactory)
        {
            _tokens = new List<StoredToken>();
            _identityServerClientFactory = identityServerClientFactory;
        }

        public async Task<GrantedToken> GetToken(string url, string clientId, string clientSecret, IEnumerable<string> scopes)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (scopes == null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            var token = _tokens.FirstOrDefault(t => t.Url == url && scopes.Count() == t.Scopes.Count() && scopes.All(s => t.Scopes.Contains(s)));
            if (token != null)
            {
                if (DateTime.UtcNow < token.ExpirationDateTime)
                {
                    return token.GrantedToken;
                }

                _tokens.Remove(token);
            }

            var grantedToken = await _identityServerClientFactory.CreateAuthSelector()
                .UseClientSecretPostAuth(clientId, clientSecret)
                .UseClientCredentials(scopes.ToArray())
                .ResolveAsync(url)
                .ConfigureAwait(false);
            _tokens.Add(new StoredToken
            {
                GrantedToken = grantedToken,
                ExpirationDateTime = DateTime.UtcNow.AddSeconds(grantedToken.ExpiresIn),
                Scopes = scopes,
                Url = url
            });

            return grantedToken;
        }
    }
}
