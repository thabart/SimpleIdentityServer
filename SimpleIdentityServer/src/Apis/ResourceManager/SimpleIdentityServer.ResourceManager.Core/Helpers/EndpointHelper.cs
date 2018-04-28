using SimpleIdentityServer.ResourceManager.Core.Exceptions;
using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Parameters;
using SimpleIdentityServer.ResourceManager.Core.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Core.Helpers
{
    public interface IEndpointHelper
    {
        Task<EndpointAggregate> TryGetEndpointFromProfile(string subject, EndpointTypes type);
        Task<EndpointAggregate> TryGetEndpoint(string url, EndpointTypes type);
        Task<EndpointAggregate> GetEndpoint(string url, EndpointTypes type);
    }

    internal sealed class EndpointHelper : IEndpointHelper
    {
        private readonly IEndpointRepository _endpointRepository;
        private readonly IProfileRepository _profileRepository;

        public EndpointHelper(IEndpointRepository endpointRepository, IProfileRepository profileRepository)
        {
            _endpointRepository = endpointRepository;
            _profileRepository = profileRepository;
        }

        public async Task<EndpointAggregate> TryGetEndpointFromProfile(string subject, EndpointTypes type)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            var profile = await _profileRepository.Get(subject);
            if (profile == null)
            {
                return await TryGetEndpoint(null, type);
            }
            
            switch(type)
            {
                case EndpointTypes.AUTH:
                    return await TryGetEndpoint(profile.AuthUrl, type);
                case EndpointTypes.OPENID:
                    return await TryGetEndpoint(profile.OpenidUrl, type);
                case EndpointTypes.SCIM:
                    return await TryGetEndpoint(profile.ScimUrl, type);
            }

            return await TryGetEndpoint(null, type);
        }

        public async Task<EndpointAggregate> TryGetEndpoint(string url, EndpointTypes type)
        {
            var endpoint = await GetEndpoint(url, type);
            if (endpoint == null)
            {
                throw new ResourceManagerInternalException(Constants.ErrorCodes.InternalError, Constants.Errors.ErrNoEndpoint);
            }

            if (string.IsNullOrWhiteSpace(endpoint.AuthUrl) || string.IsNullOrWhiteSpace(endpoint.ClientId) || string.IsNullOrWhiteSpace(endpoint.ClientSecret))
            {
                throw new ResourceManagerInternalException(Constants.ErrorCodes.InternalError, Constants.Errors.ErrAuthNotConfigured);
            }

            if (string.IsNullOrWhiteSpace(endpoint.ManagerUrl))
            {
                throw new ResourceManagerInternalException(Constants.ErrorCodes.InternalError, Constants.Errors.ErrManagerApiNotConfigured);
            }

            return endpoint;
        }

        public async Task<EndpointAggregate> GetEndpoint(string url, EndpointTypes type)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                var endpoint = await _endpointRepository.Get(url);
                return endpoint;
            }

            
            var endpoints = await _endpointRepository.Search(new SearchEndpointsParameter
            {
                Type = type
            });
            if (endpoints == null || !endpoints.Any())
            {
                return null;
            }

            var minOrder = endpoints.Min(e => e.Order);
            return endpoints.First(e => e.Order == minOrder);
        }
    }
}
