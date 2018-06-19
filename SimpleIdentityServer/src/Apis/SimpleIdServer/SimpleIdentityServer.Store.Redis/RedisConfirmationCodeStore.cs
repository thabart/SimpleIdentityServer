using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Store.Redis
{
    internal sealed class RedisConfirmationCodeStore : IConfirmationCodeStore
    {
        private readonly RedisStorage _redisStorage;

        public RedisConfirmationCodeStore(RedisStorage redisStorage)
        {
            _redisStorage = redisStorage;
        }

        public Task<bool> Add(ConfirmationCode confirmationCode)
        {
            throw new NotImplementedException();
        }

        public Task<ConfirmationCode> Get(string code)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Remove(string code)
        {
            throw new NotImplementedException();
        }
    }
}
