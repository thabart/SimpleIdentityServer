using SimpleIdentityServer.Core.Models;
using System.Collections.Generic;

namespace SimpleIdentityServer.Core.Repositories
{
    public interface IConsentRepository
    {
        List<Consent> GetConsentsForGivenUser(string subject);

        void InsertConsent(Consent record);
    }
}
