using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Core.Helpers;
using System;

namespace SimpleIdentityServer.Core
{
    public static class SimpleIdentityServerCoreExtensions
    {
        public static IServiceCollection AddSimpleIdentityServerCore(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException("register");
            }

            serviceCollection.AddTransient<ISecurityHelper, SecurityHelper>();

            /*
            serviceCollection.TryAdd<ISecurityHelper, SecurityHelper>();
            register.RegisterType<IGrantedTokenGeneratorHelper, GrantedTokenGeneratorHelper>();
            register.RegisterType<IConsentHelper, ConsentHelper>();
            register.RegisterType<IClientHelper, ClientHelper>();
            register.RegisterType<IAuthorizationFlowHelper, AuthorizationFlowHelper>();
            register.RegisterType<IClientValidator, ClientValidator>();
            register.RegisterType<IResourceOwnerValidator, ResourceOwnerValidator>();
            register.RegisterType<IScopeValidator, ScopeValidator>();
            register.RegisterType<IGrantedTokenValidator, GrantedTokenValidator>();
            register.RegisterType<IAuthorizationCodeGrantTypeParameterAuthEdpValidator, AuthorizationCodeGrantTypeParameterAuthEdpValidator>();
            register.RegisterType<IResourceOwnerGrantTypeParameterValidator, ResourceOwnerGrantTypeParameterValidator>();
            register.RegisterType<IAuthorizationCodeGrantTypeParameterTokenEdpValidator,
                AuthorizationCodeGrantTypeParameterTokenEdpValidator>();
            register.RegisterType<IRegistrationParameterValidator, RegistrationParameterValidator>();
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
            register.RegisterType<IGetAuthorizationCodeAndTokenViaHybridWorkflowOperation, GetAuthorizationCodeAndTokenViaHybridWorkflowOperation>();

            register.RegisterType<IConsentActions, ConsentActions>();
            register.RegisterType<IConfirmConsentAction, ConfirmConsentAction>();
            register.RegisterType<IDisplayConsentAction, DisplayConsentAction>();

            register.RegisterType<IRegisterClientAction, RegisterClientAction>();
            register.RegisterType<IRegistrationActions, RegistrationActions>();

            register.RegisterType<IJwksActions, JwksActions>();
            register.RegisterType<IRotateJsonWebKeysOperation, RotateJsonWebKeysOperation>();
            register.RegisterType<IGetSetOfPublicKeysUsedToValidateJwsAction, GetSetOfPublicKeysUsedToValidateJwsAction>();
            register.RegisterType<IJsonWebKeyEnricher, JsonWebKeyEnricher>();
            register.RegisterType<IGetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction, GetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction>();

            register.RegisterType<IAuthenticateActions, AuthenticateActions>();
            register.RegisterType<IAuthenticateResourceOwnerAction, AuthenticateResourceOwnerAction>();
            register.RegisterType<IAuthenticateHelper, AuthenticateHelper>();
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

            register.RegisterType<IGetTokenByRefreshTokenGrantTypeAction, GetTokenByRefreshTokenGrantTypeAction>();
            register.RegisterType<IRefreshTokenGrantTypeParameterValidator, RefreshTokenGrantTypeParameterValidator>();

            register.RegisterType<ITranslationManager, TranslationManager>();

            register.RegisterType<IHttpClientFactory, HttpClientFactory>();*/
            return serviceCollection;
        }
    }
}
