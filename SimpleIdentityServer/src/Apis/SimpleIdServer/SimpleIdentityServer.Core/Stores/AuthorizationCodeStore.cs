using SimpleIdentityServer.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Stores
{
    public interface IAuthorizationCodeStore
    {
        Task<AuthorizationCode> GetAuthorizationCode(string code);
        Task<bool> AddAuthorizationCode(AuthorizationCode authorizationCode);
        Task<bool> RemoveAuthorizationCode(string code);
    }

    internal sealed class InMemoryAuthorizationCodeStore : IAuthorizationCodeStore
    {
        private static Dictionary<string, AuthorizationCode> _mappingStringToAuthCodes;

        public InMemoryAuthorizationCodeStore()
        {
            _mappingStringToAuthCodes = new Dictionary<string, AuthorizationCode>();
        }

        public Task<AuthorizationCode> GetAuthorizationCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (!_mappingStringToAuthCodes.ContainsKey(code))
            {
                return null;
            }

            return Task.FromResult(_mappingStringToAuthCodes[code]);
        }

        public Task<bool> AddAuthorizationCode(AuthorizationCode authorizationCode)
        {
            if (authorizationCode == null)
            {
                throw new ArgumentNullException(nameof(authorizationCode));
            }

            if (_mappingStringToAuthCodes.ContainsKey(authorizationCode.Code))
            {
                return Task.FromResult(false);
            }

            _mappingStringToAuthCodes.Add(authorizationCode.Code, authorizationCode);
            return Task.FromResult(true);
        }

        public Task<bool> RemoveAuthorizationCode(string authorizationCode)
        {
            if (authorizationCode == null)
            {
                throw new ArgumentNullException(nameof(authorizationCode));
            }

            if (!_mappingStringToAuthCodes.ContainsKey(authorizationCode))
            {
                return Task.FromResult(false);
            }

            _mappingStringToAuthCodes.Remove(authorizationCode);
            return Task.FromResult(true);
        }
    }
}
