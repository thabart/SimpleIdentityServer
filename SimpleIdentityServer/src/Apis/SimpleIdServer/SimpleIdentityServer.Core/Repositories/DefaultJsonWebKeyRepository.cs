using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Repositories
{
    internal sealed class DefaultJsonWebKeyRepository : IJsonWebKeyRepository
    {
        public ICollection<JsonWebKey> _jsonWebKeys;

        public DefaultJsonWebKeyRepository(ICollection<JsonWebKey> jsonWebKeys)
        {
            if (jsonWebKeys != null)
            {
                _jsonWebKeys = jsonWebKeys;
                return;
            }

            var serializedRsa = string.Empty;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (var provider = new RSACryptoServiceProvider())
                {
                    serializedRsa = provider.ToXmlStringNetCore(true);
                }
            }
            else
            {
                using (var rsa = new RSAOpenSsl())
                {
                    serializedRsa = rsa.ToXmlStringNetCore(true);
                }
            }

            _jsonWebKeys = new List<JsonWebKey>
            {
                new JsonWebKey
                {
                    Alg = AllAlg.RS256,
                    KeyOps = new []
                    {
                        KeyOperations.Sign,
                        KeyOperations.Verify
                    },
                    Kid = "1",
                    Kty = KeyType.RSA,
                    Use = Use.Sig,
                    SerializedKey = serializedRsa,
                },
                new JsonWebKey
                {
                    Alg = AllAlg.RSA1_5,
                    KeyOps = new []
                    {
                        KeyOperations.Encrypt,
                        KeyOperations.Decrypt
                    },
                    Kid = "2",
                    Kty = KeyType.RSA,
                    Use = Use.Enc,
                    SerializedKey = serializedRsa,
                }
            };
        }

        public Task<bool> DeleteAsync(JsonWebKey newJsonWebKey)
        {
            if (newJsonWebKey == null)
            {
                throw new ArgumentNullException(nameof(newJsonWebKey));
            }

            var jsonWebKey = _jsonWebKeys.FirstOrDefault(j => j.Kid == newJsonWebKey.Kid);
            if (jsonWebKey == null)
            {
                return Task.FromResult(false);
            }

            _jsonWebKeys.Remove(jsonWebKey);
            return Task.FromResult(true);
        }

        public Task<ICollection<JsonWebKey>> GetAllAsync()
        {
            ICollection<JsonWebKey> res = _jsonWebKeys.Select(j => j.Copy()).ToList();
            return Task.FromResult(res);
        }

        public Task<ICollection<JsonWebKey>> GetByAlgorithmAsync(Use use, AllAlg algorithm, KeyOperations[] operations)
        {
            ICollection<JsonWebKey> result = _jsonWebKeys
                .Where(j => j.Use == use && j.Alg == algorithm && operations.All(op => j.KeyOps.Contains(op)))
                .Select(j => j.Copy()).ToList();
            return Task.FromResult(result);
        }

        public Task<JsonWebKey> GetByKidAsync(string kid)
        {
            if (string.IsNullOrWhiteSpace(kid))
            {
                throw new ArgumentNullException(nameof(kid));
            }

            var res = _jsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            if (res == null)
            {
                return Task.FromResult((JsonWebKey)null);
            }

            return Task.FromResult(res.Copy());
        }

        public Task<bool> InsertAsync(JsonWebKey jsonWebKey)
        {
            if (jsonWebKey == null)
            {
                throw new ArgumentNullException(nameof(jsonWebKey));
            }
            
            _jsonWebKeys.Add(jsonWebKey.Copy());
            return Task.FromResult(true);
        }

        public Task<bool> UpdateAsync(JsonWebKey jsonWebKey)
        {
            if (jsonWebKey == null)
            {
                throw new ArgumentNullException(nameof(jsonWebKey));
            }

            var rec = _jsonWebKeys.FirstOrDefault(j => j.Kid == jsonWebKey.Kid);
            if (rec == null)
            {
                return Task.FromResult(false);
            }

            rec.KeyOps = jsonWebKey.KeyOps;
            rec.Kty = jsonWebKey.Kty;
            rec.Use = jsonWebKey.Use;
            rec.X5t = jsonWebKey.X5t;
            rec.X5tS256 = jsonWebKey.X5tS256;
            rec.X5u = jsonWebKey.X5u;
            rec.SerializedKey = jsonWebKey.SerializedKey;
            return Task.FromResult(true);
        }
    }
}
