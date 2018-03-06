using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Stores;
using SimpleIdentityServer.Uma.Common;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Core.Policies;
using SimpleIdentityServer.Uma.Core.Services;
using SimpleIdentityServer.Uma.Core.Stores;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.Token.Actions
{
    public interface IGetTokenByTicketIdAction
    {
        Task<GrantedToken> Execute(GetTokenViaTicketIdParameter parameter, AuthenticationHeaderValue authenticationHeaderValue);
    }

    internal sealed class GetTokenByTicketIdAction : IGetTokenByTicketIdAction
    {
        private readonly ITicketStore _ticketStore;
        private readonly IConfigurationService _configurationService;
        private readonly IUmaServerEventSource _umaServerEventSource;
        private readonly IIdentityServerClientFactory _identityServerClientFactory;
        private readonly IAuthorizationPolicyValidator _authorizationPolicyValidator;
        private readonly IAuthenticateInstructionGenerator _authenticateInstructionGenerator;
        private readonly IAuthenticateClient _authenticateClient;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IClientHelper _clientHelper;
        private readonly ITokenStore _tokenStore;

        public GetTokenByTicketIdAction(ITicketStore ticketStore, IConfigurationService configurationService,
            IUmaServerEventSource umaServerEventSource, IIdentityServerClientFactory identityServerClientFactory,
            IAuthorizationPolicyValidator authorizationPolicyValidator, IAuthenticateInstructionGenerator authenticateInstructionGenerator,
            IAuthenticateClient authenticateClient, IJwtGenerator jwtGenerator, IClientHelper clientHelper, ITokenStore tokenStore)
        {
            _ticketStore = ticketStore;
            _configurationService = configurationService;
            _umaServerEventSource = umaServerEventSource;
            _identityServerClientFactory = identityServerClientFactory;
            _authorizationPolicyValidator = authorizationPolicyValidator;
            _authenticateInstructionGenerator = authenticateInstructionGenerator;
            _authenticateClient = authenticateClient;
            _jwtGenerator = jwtGenerator;
            _clientHelper = clientHelper;
            _tokenStore = tokenStore;
        }

        public async Task<GrantedToken> Execute(GetTokenViaTicketIdParameter parameter, AuthenticationHeaderValue authenticationHeaderValue)
        {
            // 1. Check parameters.
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (string.IsNullOrWhiteSpace(parameter.Ticket))
            {
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, PostAuthorizationNames.TicketId));
            }

            if (string.IsNullOrWhiteSpace(parameter.Ticket))
            {
                throw new ArgumentNullException(nameof(parameter.Ticket));
            }

            // 2. Try to authenticate the client.
            var instruction = CreateAuthenticateInstruction(parameter, authenticationHeaderValue);
            var authResult = await _authenticateClient.AuthenticateAsync(instruction);
            var client = authResult.Client;
            if (client == null)
            {
                throw new BaseUmaException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            if (client.GrantTypes == null ||
                !client.GrantTypes.Contains(GrantType.client_credentials))
            {
                throw new BaseUmaException(ErrorCodes.InvalidGrant,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType, client.ClientId, GrantType.ticket_id));
            }

            // 3. Retrieve the ticket.
            var json = JsonConvert.SerializeObject(parameter);
            _umaServerEventSource.StartGettingAuthorization(json);
            var ticket = await _ticketStore.GetAsync(parameter.Ticket);
            if (ticket == null)
            {
                throw new BaseUmaException(ErrorCodes.InvalidTicket,
                    string.Format(ErrorDescriptions.TheTicketDoesntExist, parameter.Ticket));
            }

            // 4. Check the ticket.
            if (ticket.ExpirationDateTime < DateTime.UtcNow)
            {
                throw new BaseUmaException(ErrorCodes.ExpiredTicket, ErrorDescriptions.TheTicketIsExpired);
            }

            if (ticket.ClientId != client.ClientId)
            {
                throw new BaseUmaException(ErrorCodes.InvalidTicket, ErrorDescriptions.TheTicketIssuerIsDifferentFromTheClient);
            }

            _umaServerEventSource.CheckAuthorizationPolicy(json);
            var claimTokenParameter = new ClaimTokenParameter
            {
                Token = parameter.ClaimToken,
                Format = parameter.ClaimTokenFormat
            };

            // 4. Check the authorization.
            var authorizationResult = await _authorizationPolicyValidator.IsAuthorized(ticket, client.ClientId, claimTokenParameter);
            if (authorizationResult.Type != AuthorizationPolicyResultEnum.Authorized)
            {
                _umaServerEventSource.RequestIsNotAuthorized(json);
                throw new BaseUmaException(ErrorCodes.InvalidGrant, ErrorDescriptions.TheAuthorizationPolicyIsNotSatisfied);
            }

            // 5. Generate a granted token.
            var grantedToken = await GenerateTokenAsync(client, ticket.Lines, "openid");
            await _tokenStore.AddToken(grantedToken);
            await _ticketStore.RemoveAsync(ticket.Id);
            return grantedToken;
        }

        private AuthenticateInstruction CreateAuthenticateInstruction(GetTokenViaTicketIdParameter authorizationCodeGrantTypeParameter, AuthenticationHeaderValue authenticationHeaderValue)
        {
            var result = _authenticateInstructionGenerator.GetAuthenticateInstruction(authenticationHeaderValue);
            result.ClientAssertion = authorizationCodeGrantTypeParameter.ClientAssertion;
            result.ClientAssertionType = authorizationCodeGrantTypeParameter.ClientAssertionType;
            result.ClientIdFromHttpRequestBody = authorizationCodeGrantTypeParameter.ClientId;
            result.ClientSecretFromHttpRequestBody = authorizationCodeGrantTypeParameter.ClientSecret;
            return result;
        }

        public async Task<GrantedToken> GenerateTokenAsync(SimpleIdentityServer.Core.Models.Client client, IEnumerable<TicketLine> ticketLines, string scope)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (ticketLines == null)
            {
                throw new ArgumentNullException(nameof(ticketLines));
            }

            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentNullException(nameof(scope));
            }

            var expiresIn = await _configurationService.GetRptLifeTime(); // 1. Retrieve the expiration time of the granted token.
            var jwsPayload = await _jwtGenerator.GenerateAccessToken(client, scope.Split(' ')); // 2. Construct the JWT token (client).
            var jArr = new JArray();
            foreach (var ticketLine in ticketLines)
            {
                var jObj = new JObject();
                jObj.Add(Constants.RptClaims.ResourceSetId, ticketLine.ResourceSetId);
                jObj.Add(Constants.RptClaims.Scopes, string.Join(" ",ticketLine.Scopes));
                jArr.Add(jObj);
            }

            jwsPayload.Add(Constants.RptClaims.Ticket, jArr);
            var accessToken = await _clientHelper.GenerateIdTokenAsync(client, jwsPayload);
            var refreshTokenId = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()); // 3. Construct the refresh token.
            return new GrantedToken
            {
                AccessToken = accessToken,
                RefreshToken = Convert.ToBase64String(refreshTokenId),
                ExpiresIn = expiresIn,
                TokenType =SimpleIdentityServer.Core.Constants.StandardTokenTypes.Bearer,
                CreateDateTime = DateTime.UtcNow,
                Scope = scope,
                ClientId = client.ClientId
            };
        }
    }
}
