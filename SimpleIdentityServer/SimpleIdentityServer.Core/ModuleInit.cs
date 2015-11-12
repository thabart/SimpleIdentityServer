using System.ComponentModel.Composition;

using SimpleIdentityServer.Common;
using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Api.Authorization.Actions;
using SimpleIdentityServer.Core.Api.Authorization.Common;
using SimpleIdentityServer.Core.Api.Discovery;
using SimpleIdentityServer.Core.Api.Discovery.Actions;
using SimpleIdentityServer.Core.Api.Token;
using SimpleIdentityServer.Core.Api.Token.Actions;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Core.WebSite.Consent;
using SimpleIdentityServer.Core.WebSite.Consent.Actions;

namespace SimpleIdentityServer.Core
{
    [Export(typeof(IModule))]
    public class ModuleInit : IModule
    {
        public void Initialize(IModuleRegister register)
        {
            register.RegisterType<ISecurityHelper, SecurityHelper>();
            register.RegisterType<ITokenHelper, TokenHelper>();
            register.RegisterType<IClientValidator, ClientValidator>();
            register.RegisterType<IResourceOwnerValidator, ResourceOwnerValidator>();
            register.RegisterType<IScopeValidator, ScopeValidator>();
            register.RegisterType<IAuthorizationCodeGrantTypeParameterValidator, AuthorizationCodeGrantTypeParameterValidator>();
            register.RegisterType<IResourceOwnerGrantTypeParameterValidator, ResourceOwnerGrantTypeParameterValidator>();
            register.RegisterType<IProtector, Protector.Protector>();
            register.RegisterType<ICompressor, Compressor>();
            register.RegisterType<IEncoder, Encoder>();
            register.RegisterType<IParameterParserHelper, ParameterParserHelper>();
            register.RegisterType<IActionResultFactory, ActionResultFactory>();

            register
                .RegisterType<IAuthorizationActions, AuthorizationActions>
                ();
            register
                .RegisterType<IGetAuthorizationCodeOperation, GetAuthorizationCodeOperation>
                ();
            register.RegisterType<IGetTokenViaImplicitWorkflowOperation, GetTokenViaImplicitWorkflowOperation>();

            register.RegisterType<ITokenActions, TokenActions>();
            register.RegisterType<IGetTokenByResourceOwnerCredentialsGrantTypeAction, GetTokenByResourceOwnerCredentialsGrantTypeAction>();

            register.RegisterType<IConsentActions, ConsentActions>();
            register.RegisterType<IConfirmConsentAction, ConfirmConsentAction>();
            register.RegisterType<IDisplayConsentAction, DisplayConsentAction>();

            register.RegisterType<IAuthenticateActions, AuthenticateActions>();
            register.RegisterType<IAuthenticateResourceOwnerAction, AuthenticateResourceOwnerAction>();
            register.RegisterType<ILocalUserAuthenticationAction, LocalUserAuthenticationAction>();

            register.RegisterType<IDiscoveryActions, DiscoveryActions>();
            register.RegisterType<ICreateDiscoveryDocumentationAction, CreateDiscoveryDocumentationAction>();

            register.RegisterType<IProcessAuthorizationRequest, ProcessAuthorizationRequest>();

            register.RegisterType<IJwsGenerator, JwsGenerator>();
            register.RegisterType<ISimpleIdentityServerConfigurator, SimpleIdentityServerConfigurator>();
            register.RegisterType<ICreateJwsSignature, CreateJwsSignature>();
            register.RegisterType<IGenerateAuthorizationResponse, GenerateAuthorizationResponse>();
        }
    }
}
