using SimpleIdentityServer.Core.Api.Profile.Actions;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Profile
{
    public interface IProfileActions
    {
        Task<bool> Unlink(string localSubject, string externalSubject);
        Task<bool> Link(string localSubject, string externalSubject, string issuer, bool force = false);
        Task<IEnumerable<ResourceOwnerProfile>> GetProfiles(string subject);
        Task<ResourceOwner> GetResourceOwner(string externalSubject);
    }

    internal sealed class ProfileActions : IProfileActions
    {
        private readonly IUnlinkProfileAction _unlinkProfileAction;
        private readonly ILinkProfileAction _linkProfileAction;
        private readonly IGetUserProfilesAction _getUserProfilesAction;
        private readonly IGetResourceOwnerClaimsAction _getResourceOwnerClaimsAction;

        public ProfileActions(IUnlinkProfileAction unlinkProfileAction, ILinkProfileAction linkProfileAction,
            IGetUserProfilesAction getProfileAction, IGetResourceOwnerClaimsAction getResourceOwnerClaimsAction)
        {
            _unlinkProfileAction = unlinkProfileAction;
            _linkProfileAction = linkProfileAction;
            _getUserProfilesAction = getProfileAction;
            _getResourceOwnerClaimsAction = getResourceOwnerClaimsAction;
        }

        public Task<bool> Unlink(string localSubject, string externalSubject)
        {
            return _unlinkProfileAction.Execute(localSubject, externalSubject);
        }

        public Task<bool> Link(string localSubject, string externalSubject, string issuer, bool force = false)
        {
            if (string.IsNullOrWhiteSpace(externalSubject))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "user_id"));
            }

            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.MissingParameter, "issuer"));
            }

            return _linkProfileAction.Execute(localSubject, externalSubject, issuer, force);
        }

        public Task<IEnumerable<ResourceOwnerProfile>> GetProfiles(string subject)
        {
            return _getUserProfilesAction.Execute(subject);
        }

        public Task<ResourceOwner> GetResourceOwner(string externalSubject)
        {
            return _getResourceOwnerClaimsAction.Execute(externalSubject);
        }
    }
}
