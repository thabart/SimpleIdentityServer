using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Parameters;
using SimpleIdentityServer.ResourceManager.Core.Repositories;
using SimpleIdentityServer.ResourceManager.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.EF.Repositories
{
    internal sealed class EndpointRepository : IEndpointRepository
    {
        private readonly IServiceProvider _serviceProvider;

        public EndpointRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<EndpointAggregate> Get(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            using (var serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ResourceManagerDbContext>())
                {
                    var idProvider = await context.Endpoints.FirstOrDefaultAsync(a => a.Url == url);
                    if (idProvider == null)
                    {
                        return null;
                    }

                    return GetIdProvider(idProvider);
                }
            }
        }

        public async Task<bool> Add(IEnumerable<EndpointAggregate> idProviders)
        {
            if (idProviders == null)
            {
                throw new ArgumentNullException(nameof(idProviders));
            }

            using (var serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ResourceManagerDbContext>())
                {
                    using (var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false))
                    {
                        try
                        {
                            foreach (var idProvider in idProviders)
                            {
                                var record = new Endpoint
                                {
                                    CreateDateTime = idProvider.CreateDateTime,
                                    Description = idProvider.Description,
                                    Name = idProvider.Name,
                                    Url = idProvider.Url,
                                    Type = (int)idProvider.Type
                                };

                                context.Endpoints.Add(record);
                            }

                            await context.SaveChangesAsync().ConfigureAwait(false);
                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                        }
                    }

                    return false;
                }
            }
        }

        public async Task<IEnumerable<EndpointAggregate>> GetAll()
        {
            using (var serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ResourceManagerDbContext>())
                {
                    var idProviders = await context.Endpoints.ToListAsync().ConfigureAwait(false);
                    if (idProviders == null)
                    {
                        return null;
                    }

                    return idProviders.Select(i => GetIdProvider(i));
                }
            }
        }

        public async Task<IEnumerable<EndpointAggregate>> Search(SearchEndpointsParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            using (var serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ResourceManagerDbContext>())
                {
                    IQueryable<Endpoint> endpoints = context.Endpoints;
                    if (parameter.Type != null)
                    {
                        endpoints = endpoints.Where(e => e.Type == (int)parameter.Type.Value);
                    }

                    return endpoints.Select(i => GetIdProvider(i));
                }
            }
        }

        private static EndpointAggregate GetIdProvider(Endpoint idProvider)
        {
            return new EndpointAggregate
            {
                CreateDateTime = idProvider.CreateDateTime,
                Description = idProvider.Description,
                Url = idProvider.Url,
                Name = idProvider.Name,
                Type = (EndpointTypes)idProvider.Type
            };
        }
    }
}
