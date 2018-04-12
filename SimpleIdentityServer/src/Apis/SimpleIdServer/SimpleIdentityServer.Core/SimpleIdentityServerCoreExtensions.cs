#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.Extensions.DependencyInjection;
using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Api.Authorization.Actions;
using SimpleIdentityServer.Core.Api.Authorization.Common;
using SimpleIdentityServer.Core.Api.Discovery;
using SimpleIdentityServer.Core.Api.Discovery.Actions;
using SimpleIdentityServer.Core.Api.Introspection;
using SimpleIdentityServer.Core.Api.Introspection.Actions;
using SimpleIdentityServer.Core.Api.Jwks;
using SimpleIdentityServer.Core.Api.Jwks.Actions;
using SimpleIdentityServer.Core.Api.Registration;
using SimpleIdentityServer.Core.Api.Registration.Actions;
using SimpleIdentityServer.Core.Api.Token;
using SimpleIdentityServer.Core.Api.Token.Actions;
using SimpleIdentityServer.Core.Api.UserInfo;
using SimpleIdentityServer.Core.Api.UserInfo.Actions;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Protector;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.Stores;
using SimpleIdentityServer.Core.Translation;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Core.WebSite.Account;
using SimpleIdentityServer.Core.WebSite.Account.Actions;
using SimpleIdentityServer.Core.WebSite.Authenticate;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Core.WebSite.Authenticate.Common;
using SimpleIdentityServer.Core.WebSite.Consent;
using SimpleIdentityServer.Core.WebSite.Consent.Actions;
using SimpleIdentityServer.Core.WebSite.User;
using SimpleIdentityServer.Core.WebSite.User.Actions;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Core
{
    public static class SimpleIdentityServerCoreExtensions
    {
        public static IServiceCollection AddInMemoryStorage(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<IAuthorizationCodeStore>(new InMemoryAuthorizationCodeStore());
            serviceCollection.AddSingleton<ITokenStore>(new InMemoryTokenStore());
            return serviceCollection;
        }

        public static IServiceCollection AddSimpleIdentityServerCore(this IServiceCollection serviceCollection, IHttpClientFactory factory = null)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException("serviceCollection");
            }

			serviceCollection.AddTransient<IGrantedTokenGeneratorHelper, GrantedTokenGeneratorHelper>();
            serviceCollection.AddTransient<IConsentHelper, ConsentHelper>();
            serviceCollection.AddTransient<IClientHelper, ClientHelper>();
            serviceCollection.AddTransient<IAuthorizationFlowHelper, AuthorizationFlowHelper>();
            serviceCollection.AddTransient<IClientCredentialsGrantTypeParameterValidator, ClientCredentialsGrantTypeParameterValidator>();
            serviceCollection.AddTransient<IClientValidator, ClientValidator>();
            serviceCollection.AddTransient<IScopeValidator, ScopeValidator>();
            serviceCollection.AddTransient<IGrantedTokenValidator, GrantedTokenValidator>();
            serviceCollection.AddTransient<IAuthorizationCodeGrantTypeParameterAuthEdpValidator, AuthorizationCodeGrantTypeParameterAuthEdpValidator>();
            serviceCollection.AddTransient<IResourceOwnerGrantTypeParameterValidator, ResourceOwnerGrantTypeParameterValidator>();
            serviceCollection.AddTransient<IAuthorizationCodeGrantTypeParameterTokenEdpValidator, AuthorizationCodeGrantTypeParameterTokenEdpValidator>();
            serviceCollection.AddTransient<IRegistrationParameterValidator, RegistrationParameterValidator>();
            serviceCollection.AddTransient<ICompressor, Compressor>();
            serviceCollection.AddTransient<IEncoder, Encoder>();
            serviceCollection.AddTransient<IParameterParserHelper, ParameterParserHelper>();
            serviceCollection.AddTransient<IActionResultFactory, ActionResultFactory>();
            serviceCollection.AddTransient<IAuthorizationActions, AuthorizationActions>();
            serviceCollection.AddTransient<IGetAuthorizationCodeOperation, GetAuthorizationCodeOperation>();
            serviceCollection.AddTransient<IGetTokenViaImplicitWorkflowOperation, GetTokenViaImplicitWorkflowOperation>();
            serviceCollection.AddTransient<IUserInfoActions, UserInfoActions>();
            serviceCollection.AddTransient<IGetJwsPayload, GetJwsPayload>();
            serviceCollection.AddTransient<ITokenActions, TokenActions>();
            serviceCollection.AddTransient<IGetTokenByResourceOwnerCredentialsGrantTypeAction, GetTokenByResourceOwnerCredentialsGrantTypeAction>();
            serviceCollection.AddTransient<IGetTokenByAuthorizationCodeGrantTypeAction, GetTokenByAuthorizationCodeGrantTypeAction>();
            serviceCollection.AddTransient<IGetAuthorizationCodeAndTokenViaHybridWorkflowOperation, GetAuthorizationCodeAndTokenViaHybridWorkflowOperation>();
            serviceCollection.AddTransient<IGetTokenByClientCredentialsGrantTypeAction, GetTokenByClientCredentialsGrantTypeAction>();
            serviceCollection.AddTransient<IConsentActions, ConsentActions>();
            serviceCollection.AddTransient<IConfirmConsentAction, ConfirmConsentAction>();
            serviceCollection.AddTransient<IDisplayConsentAction, DisplayConsentAction>();
            serviceCollection.AddTransient<IRegistrationActions, RegistrationActions>();
            serviceCollection.AddTransient<IJwksActions, JwksActions>();
            serviceCollection.AddTransient<IRotateJsonWebKeysOperation, RotateJsonWebKeysOperation>();
            serviceCollection.AddTransient<IGetSetOfPublicKeysUsedToValidateJwsAction, GetSetOfPublicKeysUsedToValidateJwsAction>();
            serviceCollection.AddTransient<IJsonWebKeyEnricher, JsonWebKeyEnricher>();
            serviceCollection.AddTransient<IGetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction, GetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction>();
            serviceCollection.AddTransient<IAuthenticateActions, AuthenticateActions>();
            serviceCollection.AddTransient<IAuthenticateResourceOwnerOpenIdAction, AuthenticateResourceOwnerOpenIdAction>();
            serviceCollection.AddTransient<ILocalOpenIdUserAuthenticationAction, LocalOpenIdUserAuthenticationAction>();
            serviceCollection.AddTransient<IExternalOpenIdUserAuthenticationAction, ExternalOpenIdUserAuthenticationAction>();
            serviceCollection.AddTransient<IAuthenticateHelper, AuthenticateHelper>();
            serviceCollection.AddTransient<ILocalUserAuthenticationAction, LocalUserAuthenticationAction>();
            serviceCollection.AddTransient<IDiscoveryActions, DiscoveryActions>();
            serviceCollection.AddTransient<ICreateDiscoveryDocumentationAction, CreateDiscoveryDocumentationAction>();
            serviceCollection.AddTransient<IProcessAuthorizationRequest, ProcessAuthorizationRequest>();
            serviceCollection.AddTransient<IJwtGenerator, JwtGenerator>();
            serviceCollection.AddTransient<IJwtParser, JwtParser>();
            serviceCollection.AddTransient<IGenerateAuthorizationResponse, GenerateAuthorizationResponse>();
            serviceCollection.AddTransient<IAuthenticateClient, AuthenticateClient>();
            serviceCollection.AddTransient<IAuthenticateInstructionGenerator, AuthenticateInstructionGenerator>();
            serviceCollection.AddTransient<IClientSecretBasicAuthentication, ClientSecretBasicAuthentication>();
            serviceCollection.AddTransient<IClientSecretPostAuthentication, ClientSecretPostAuthentication>();
            serviceCollection.AddTransient<IClientAssertionAuthentication, ClientAssertionAuthentication>();
            serviceCollection.AddTransient<IClientTlsAuthentication, ClientTlsAuthentication>();
            serviceCollection.AddTransient<IGetTokenByRefreshTokenGrantTypeAction, GetTokenByRefreshTokenGrantTypeAction>();
            serviceCollection.AddTransient<IRefreshTokenGrantTypeParameterValidator, RefreshTokenGrantTypeParameterValidator>();
            serviceCollection.AddTransient<ITranslationManager, TranslationManager>();
            serviceCollection.AddTransient<IGrantedTokenHelper, GrantedTokenHelper>();
            if (factory != null)
            {
                serviceCollection.AddSingleton(factory);
            }
            else
            {
                serviceCollection.AddTransient<IHttpClientFactory, HttpClientFactory>();
            }

            serviceCollection.AddTransient<IIntrospectionActions, IntrospectionActions>();
            serviceCollection.AddTransient<IPostIntrospectionAction, PostIntrospectionAction>();
            serviceCollection.AddTransient<IIntrospectionParameterValidator, IntrospectionParameterValidator>();
            serviceCollection.AddTransient<IRegisterClientAction, RegisterClientAction>();
            serviceCollection.AddTransient<IJsonWebKeyConverter, JsonWebKeyConverter>();
            serviceCollection.AddTransient<IGenerateClientFromRegistrationRequest, GenerateClientFromRegistrationRequest>();
            serviceCollection.AddTransient<IGetConsentsOperation, GetConsentsOperation>();
            serviceCollection.AddTransient<IUserActions, UserActions>();
            serviceCollection.AddTransient<IRemoveConsentOperation, RemoveConsentOperation>();
            serviceCollection.AddTransient<IRevokeTokenAction, RevokeTokenAction>();
            serviceCollection.AddTransient<ILoginCallbackAction, LoginCallbackAction>();
            serviceCollection.AddTransient<IGetUserOperation, GetUserOperation>();
            serviceCollection.AddTransient<IUpdateUserOperation, UpdateUserOperation>();
            serviceCollection.AddTransient<IAccountActions, AccountActions>();
            serviceCollection.AddTransient<IAddResourceOwnerAction, AddResourceOwnerAction>();
            serviceCollection.AddTransient<IConfirmUserOperation, ConfirmUserOperation>();
            serviceCollection.AddTransient<IGenerateAndSendCodeAction, GenerateAndSendCodeAction>();
            serviceCollection.AddTransient<IValidateConfirmationCodeAction, ValidateConfirmationCodeAction>();
            serviceCollection.AddTransient<IRemoveConfirmationCodeAction, RemoveConfirmationCodeAction>();
            serviceCollection.AddTransient<ITwoFactorAuthenticationHandler, TwoFactorAuthenticationHandler>();
            serviceCollection.AddTransient<IPayloadSerializer, PayloadSerializer>();
            return serviceCollection;
        }
    }
}
