using Microsoft.Practices.Unity;

using SimpleIdentityServer.Api.Fake.DataAccess;
using SimpleIdentityServer.Core.DataAccess;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Operations;

using System.Web.Http;

namespace SimpleIdentityServer.Api
{
    public static class UnityConfig
    {
        public static void Configure(HttpConfiguration httpConfiguration)
        {
            var container = new UnityContainer();

            container.RegisterType<IGetTokenByResourceOwnerCredentialsGrantType, GetTokenByResourceOwnerCredentialsGrantType>();
            container.RegisterType<ISecurityHelper, SecurityHelper>();
            container.RegisterType<ITokenHelper, TokenHelper>();

            container.RegisterInstance<IDataSource>(new FakeDataSource(container.Resolve<ISecurityHelper>()));

            httpConfiguration.DependencyResolver = new UnityResolver(container);
        }
    }
}