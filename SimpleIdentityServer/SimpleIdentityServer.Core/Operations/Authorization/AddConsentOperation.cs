using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;

using System;
using System.Linq;

namespace SimpleIdentityServer.Core.Operations.Authorization
{
    public interface IAddConsentOperation
    {
        Consent Execute(GetAuthorizationParameter parameter, string subject);
    }

    public class AddConsentOperation : IAddConsentOperation
    {
        private readonly IClientValidator _clientValidator;

        private readonly IScopeValidator _scopeValidator;

        private readonly IConsentRepository _consentRepository;

        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public AddConsentOperation (
            IClientValidator clientValidator,
            IScopeValidator scopeValidator,
            IConsentRepository consentRepository,
            IResourceOwnerRepository resourceOwnerRepository)
        {
            _clientValidator = clientValidator;
            _scopeValidator = scopeValidator;
            _consentRepository = consentRepository;
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public Consent Execute(GetAuthorizationParameter parameter, string subject)
        {
            parameter.Validate();
            var client = _clientValidator.ValidateClientExist(parameter.ClientId);
            _clientValidator.ValidateRedirectionUrl(parameter.RedirectUrl, client);
            var allowedScopes = _scopeValidator.ValidateAllowedScopes(parameter.Scope, client);
            var resourceOwner = _resourceOwnerRepository.GetBySubject(subject);
            // TODO : CHECK NOT NULL && return the exception

            if (!allowedScopes.Contains("openid"))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestUriCode,
                    string.Format(ErrorDescriptions.TheScopesNeedToBeSpecified, "openid"),
                    parameter.State);
            }

            var consent = new Consent
            {
                Id = Guid.NewGuid().ToString(),
                Client = client,
                GrantedScopes = allowedScopes.Select(s => new Scope
                {
                    Name = s
                }).ToList(),
                ResourceOwner = resourceOwner

            };
            _consentRepository.InsertConsent(consent);
            return consent;
        }
    }
}
