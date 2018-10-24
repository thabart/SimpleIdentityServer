using SimpleIdentityServer.Core.Common.DTOs.Responses;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.AccessToken.Store
{
    public class StoredToken
    {
        public StoredToken()
        {
            Scopes = new List<string>();
        }

        public string Url { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        public GrantedTokenResponse GrantedToken { get; set; }
        public DateTime ExpirationDateTime { get; set; }
    }
}
