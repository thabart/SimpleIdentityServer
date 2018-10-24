using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Helpers
{
    public interface IResourceOwnerAuthenticateHelper
    {
        Task<ResourceOwner> Authenticate(string login, string password, IEnumerable<string> exceptedAmrValues);
        IEnumerable<string> GetAmrs();
    }

    internal class ResourceOwnerAuthenticateHelper : IResourceOwnerAuthenticateHelper
    {
        private readonly IEnumerable<IAuthenticateResourceOwnerService> _services;
        private readonly IAmrHelper _amrHelper;

        public ResourceOwnerAuthenticateHelper(IEnumerable<IAuthenticateResourceOwnerService> services, IAmrHelper amrHelper)
        {
            _services = services;
            _amrHelper = amrHelper;
        }

        public Task<ResourceOwner> Authenticate(string login, string password, IEnumerable<string> exceptedAmrValues)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException(nameof(login));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            var currentAmrs = _services.Select(s => s.Amr);
            var amr = _amrHelper.GetAmr(currentAmrs, exceptedAmrValues);
            var service = _services.FirstOrDefault(s => s.Amr == amr);
            return service.AuthenticateResourceOwnerAsync(login, password);
        }
        
        public IEnumerable<string> GetAmrs()
        {
            return _services.Select(s => s.Amr);
        }
    }
}
