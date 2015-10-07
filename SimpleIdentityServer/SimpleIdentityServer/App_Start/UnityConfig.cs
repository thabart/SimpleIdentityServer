using Microsoft.Practices.Unity;using System;
using System.Web.Http;

using SimpleIdentityServer.Core.DataAccess;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Operations;
using SimpleIdentityServer.DataAccess.Fake;

namespace SimpleIdentityServer.Api
{
    public static class UnityConfig
    {
        private static Action<UnityContainer> _unityRegisterTypesCallback = RegisterDependencies;

        public static void SetRegisterDependenciesCallback(Action<UnityContainer> unityRegisterTypesCallback)
        {
            _unityRegisterTypesCallback = unityRegisterTypesCallback;
        }

        public static void Configure(HttpConfiguration httpConfiguration)
        {
            var container = new UnityContainer();

            container.RegisterType<IGetTokenByResourceOwnerCredentialsGrantType, GetTokenByResourceOwnerCredentialsGrantType>();
            container.RegisterType<ISecurityHelper, SecurityHelper>();
            container.RegisterType<ITokenHelper, TokenHelper>();
            container.RegisterType<IValidatorHelper, ValidatorHelper>();

            _unityRegisterTypesCallback(container);

            httpConfiguration.DependencyResolver = new UnityResolver(container);
        }

        private static void RegisterDependencies(UnityContainer container)
        {
            container.RegisterInstance<IDataSource>(new FakeDataSource(container.Resolve<ISecurityHelper>()));
        }
    }
}