using System.ComponentModel.Composition;

using SimpleIdentityServer.Common;
using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Api.Authorization.Actions;
using SimpleIdentityServer.Core.Api.Authorization.Common;
using SimpleIdentityServer.Core.Api.Discovery;
using SimpleIdentityServer.Core.Api.Discovery.Actions;
using SimpleIdentityServer.Core.Api.Jwks;
using SimpleIdentityServer.Core.Api.Jwks.Actions;
using SimpleIdentityServer.Core.Api.Token;
using SimpleIdentityServer.Core.Api.Token.Actions;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Core.WebSite.Consent;
using SimpleIdentityServer.Core.WebSite.Consent.Actions;
using SimpleIdentityServer.Core.JwtToken.Validator;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Api.UserInfo;
using SimpleIdentityServer.Core.Api.UserInfo.Actions;
using SimpleIdentityServer.Core.Translation;

namespace SimpleIdentityServer.Core
{
    [Export(typeof(IModule))]
    public class ModuleInit : IModule
    {
        public void Initialize(IModuleRegister register)
        {
            register.RegisterType<ISecurityHelper, SecurityHelper>();
            register.RegisterType<IGrantedTokenGeneratorHelper, GrantedTokenGeneratorHelper>();
            register.RegisterType<IConsentHelper, ConsentHelper>();
            register.RegisterType<IClientValidator, ClientValidator>();
            register.RegisterType<IResourceOwnerValidator, ResourceOwnerValidator>();
            register.RegisterType<IScopeValidator, ScopeValidator>();
            register.RegisterType<IGrantedTokenValidator, GrantedTokenValidator>();
            register.RegisterType<IAuthorizationCodeGrantTypeParameterAuthEdpValidator, AuthorizationCodeGrantTypeParameterAuthEdpValidator>();
            register.RegisterType<IResourceOwnerGrantTypeParameterValidator, ResourceOwnerGrantTypeParameterValidator>();
            register.RegisterType<IAuthorizationCodeGrantTypeParameterTokenEdpValidator,
                AuthorizationCodeGrantTypeParameterTokenEdpValidator>();
            register.RegisterType<IJwtClientParameterValidator, JwtClientParameterValidator>();
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

            register.RegisterType<IUserInfoActions, UserInfoActions>();
            register.RegisterType<IGetJwsPayload, GetJwsPayload>();

            register.RegisterType<ITokenActions, TokenActions>();
            register.RegisterType<IGetTokenByResourceOwnerCredentialsGrantTypeAction, GetTokenByResourceOwnerCredentialsGrantTypeAction>();
            register.RegisterType<IGetTokenByAuthorizationCodeGrantTypeAction, GetTokenByAuthorizationCodeGrantTypeAction>();

            register.RegisterType<IConsentActions, ConsentActions>();
            register.RegisterType<IConfirmConsentAction, ConfirmConsentAction>();
            register.RegisterType<IDisplayConsentAction, DisplayConsentAction>();

            register.RegisterType<IJwksActions, JwksActions>();
            register.RegisterType<IGetSetOfPublicKeysUsedToValidateJwsAction, GetSetOfPublicKeysUsedToValidateJwsAction>();
            register.RegisterType<IJsonWebKeyEnricher, JsonWebKeyEnricher>();
            register.RegisterType<IGetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction, GetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction>();

            register.RegisterType<IAuthenticateActions, AuthenticateActions>();
            register.RegisterType<IAuthenticateResourceOwnerAction, AuthenticateResourceOwnerAction>();
            register.RegisterType<ILocalUserAuthenticationAction, LocalUserAuthenticationAction>();

            register.RegisterType<IDiscoveryActions, DiscoveryActions>();
            register.RegisterType<ICreateDiscoveryDocumentationAction, CreateDiscoveryDocumentationAction>();

            register.RegisterType<IProcessAuthorizationRequest, ProcessAuthorizationRequest>();

            register.RegisterType<IJwtGenerator, JwtGenerator>();
            register.RegisterType<IJwtParser, JwtParser>();
            // register.RegisterType<ISimpleIdentityServerConfigurator, SimpleIdentityServerConfigurator>();
            register.RegisterType<IGenerateAuthorizationResponse, GenerateAuthorizationResponse>();

            register.RegisterType<IAuthenticateClient, AuthenticateClient>();
            register.RegisterType<IClientSecretBasicAuthentication, ClientSecretBasicAuthentication>();
            register.RegisterType<IClientSecretPostAuthentication, ClientSecretPostAuthentication>();
            register.RegisterType<IClientAssertionAuthentication, ClientAssertionAuthentication>();

            register.RegisterType<ITranslationManager, TranslationManager>();

            register.RegisterType<IHttpClientFactory, HttpClientFactory>();
        }
    }
}
