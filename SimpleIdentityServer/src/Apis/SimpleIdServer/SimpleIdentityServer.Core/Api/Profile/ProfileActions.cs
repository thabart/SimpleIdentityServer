using SimpleIdentityServer.Core.Api.Profile.Actions;
using SimpleIdentityServer.Core.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Profile
{
    public interface IProfileActions
    {
        Task<bool> Unlink(string localSubject, string externalSubject);
        Task<bool> Link(string localSubject, string externalSubject, string issuer, bool force = false);
        Task<IEnumerable<ResourceOwnerProfile>> Get(string subject);
    }

    internal sealed class ProfileActions : IProfileActions
    {
        private readonly IUnlinkProfileAction _unlinkProfileAction;
        private readonly ILinkProfileAction _linkProfileAction;
        private readonly IGetProfileAction _getProfileAction;

        public ProfileActions(IUnlinkProfileAction unlinkProfileAction, ILinkProfileAction linkProfileAction,
            IGetProfileAction getProfileAction)
        {
            _unlinkProfileAction = unlinkProfileAction;
            _linkProfileAction = linkProfileAction;
            _getProfileAction = getProfileAction;
        }

        public Task<bool> Unlink(string localSubject, string externalSubject)
        {
            return _unlinkProfileAction.Execute(localSubject, externalSubject);
        }

        public Task<bool> Link(string localSubject, string externalSubject, string issuer, bool force = false)
        {
            return _linkProfileAction.Execute(localSubject, externalSubject, issuer, force);
        }

        public Task<IEnumerable<ResourceOwnerProfile>> Get(string subject)
        {
            return _getProfileAction.Execute(subject);
        }
    }
}
