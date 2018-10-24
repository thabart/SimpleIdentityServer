using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Services
{
    public class DefaultSubjectBuilder : ISubjectBuilder
    {
        public Task<string> BuildSubject()
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }
    }
}