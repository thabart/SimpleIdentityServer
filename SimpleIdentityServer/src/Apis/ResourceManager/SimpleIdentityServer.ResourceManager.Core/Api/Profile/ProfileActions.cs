using SimpleIdentityServer.ResourceManager.Core.Api.Profile.Actions;
using SimpleIdentityServer.ResourceManager.Core.Models;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Core.Api.Profile
{
    public interface IProfileActions
    {
        Task<ProfileAggregate> GetProfile(string subject);
        Task<bool> Update(ProfileAggregate profile);
    }

    internal sealed class ProfileActions : IProfileActions
    {
        private readonly IGetProfileAction _getProfileAction;
        private readonly IUpdateProfileAction _updateProfileAction;

        public ProfileActions(IGetProfileAction getProfileAction, IUpdateProfileAction updateProfileAction)
        {
            _getProfileAction = getProfileAction;
            _updateProfileAction = updateProfileAction;
        }

        public Task<ProfileAggregate> GetProfile(string subject)
        {
            return _getProfileAction.Execute(subject);
        }

        public Task<bool> Update(ProfileAggregate profile)
        {
            return _updateProfileAction.Execute(profile);
        }
    }
}
