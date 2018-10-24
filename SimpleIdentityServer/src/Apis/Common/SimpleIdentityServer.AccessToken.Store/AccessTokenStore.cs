using SimpleIdentityServer.Client;
using SimpleIdentityServer.Core.Common.DTOs.Responses;
using SimpleIdServer.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.AccessToken.Store
{
    public class AccessTokenStore : IAccessTokenStore
    {
        private static string CACHE_KEY = "access_tokens";
        private readonly IIdentityServerClientFactory _identityServerClientFactory;
        private readonly IStorage _storage;

        public AccessTokenStore(IStorage storage)
        {
            _storage = storage;
            _identityServerClientFactory = new IdentityServerClientFactory();
        }

        public AccessTokenStore(IStorage storage, IIdentityServerClientFactory identityServerClientFactory)
        {
            _storage = storage;
            _identityServerClientFactory = identityServerClientFactory;
        }

        public async Task<GrantedTokenResponse> GetToken(string url, string clientId, string clientSecret, IEnumerable<string> scopes)
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
            
            var token = await GetToken(url, scopes).ConfigureAwait(false);
            if (token != null)
            {
                if (DateTime.UtcNow < token.ExpirationDateTime)
                {
                    return token.GrantedToken;
                }

                await RemoveToken(token).ConfigureAwait(false);
            }

            var grantedToken = await _identityServerClientFactory.CreateAuthSelector()
                .UseClientSecretPostAuth(clientId, clientSecret)
                .UseClientCredentials(scopes.ToArray())
                .ResolveAsync(url)
                .ConfigureAwait(false);
            var storedToken = new StoredToken
            {
                GrantedToken = grantedToken.Content,
                ExpirationDateTime = DateTime.UtcNow.AddSeconds(grantedToken.Content.ExpiresIn),
                Scopes = scopes,
                Url = url
            };
            await AddToken(storedToken).ConfigureAwait(false);
            return grantedToken.Content;
        }

        private async Task<StoredToken> GetToken(string url, IEnumerable<string> scopes)
        {
            var storedTokens = await _storage.TryGetValueAsync<List<StoredToken>>(CACHE_KEY);
            if (storedTokens == null)
            {
                return null;
            }

            return storedTokens.FirstOrDefault(t => t.Url == url && scopes.All(s => t.Scopes.Contains(s)));
        }

        private async Task AddToken(StoredToken token)
        {
            var storedTokens = await _storage.TryGetValueAsync<List<StoredToken>>(CACHE_KEY);
            if (storedTokens == null)
            {
                storedTokens = new List<StoredToken>();
            }

            storedTokens.Add(token);
            await _storage.SetAsync(CACHE_KEY, storedTokens);
        }

        private async Task RemoveToken(StoredToken token)
        {
            var storedTokens = await _storage.TryGetValueAsync<List<StoredToken>>(CACHE_KEY);
            if (storedTokens == null)
            {
                return;
            }

            var st = storedTokens.FirstOrDefault(t => t.GrantedToken.AccessToken == token.GrantedToken.AccessToken);
            if (st == null)
            {
                return;
            }

            storedTokens.Remove(st);
            await _storage.SetAsync(CACHE_KEY, storedTokens);
        }
    }
}
