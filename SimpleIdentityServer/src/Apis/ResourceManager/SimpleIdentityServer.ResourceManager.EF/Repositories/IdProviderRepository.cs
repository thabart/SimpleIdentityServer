using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Repositories;
using SimpleIdentityServer.ResourceManager.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.EF.Repositories
{
    internal sealed class IdProviderRepository : IIdProviderRepository
    {
        private readonly IServiceProvider _serviceProvider;

        public IdProviderRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        public async Task<IdProviderAggregate> Get(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            using (var serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ResourceManagerDbContext>())
                {
                    var idProvider = await context.IdProviders.FirstOrDefaultAsync(a => a.OpenIdWellKnownUrl == url);
                    if (idProvider == null)
                    {
                        return null;
                    }

                    return GetIdProvider(idProvider);
                }
            }
        }

        public async Task<bool> Add(IEnumerable<IdProviderAggregate> idProviders)
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
                                var record = new IdProvider
                                {
                                    CreateDateTime = idProvider.CreateDateTime,
                                    Description = idProvider.Description,
                                    Name = idProvider.Name,
                                    OpenIdWellKnownUrl = idProvider.OpenIdWellKnownUrl
                                };

                                context.IdProviders.Add(record);
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

        public async Task<IEnumerable<IdProviderAggregate>> GetAll()
        {
            using (var serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ResourceManagerDbContext>())
                {
                    var idProviders = await context.IdProviders.ToListAsync().ConfigureAwait(false);
                    if (idProviders == null)
                    {
                        return null;
                    }

                    return idProviders.Select(i => GetIdProvider(i));
                }
            }
        }

        private static IdProviderAggregate GetIdProvider(IdProvider idProvider)
        {
            return new IdProviderAggregate
            {
                CreateDateTime = idProvider.CreateDateTime,
                Description = idProvider.Description,
                OpenIdWellKnownUrl = idProvider.OpenIdWellKnownUrl,
                Name = idProvider.Name
            };
        }
    }
}
