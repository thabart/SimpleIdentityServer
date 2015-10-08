using TechTalk.SpecFlow;

namespace SimpleIdentityServer.Api.Tests.Common
{
    public class Transformations
    {
        [StepArgumentTransformation(@"in (\d+) days?")]
        public string InXDaysTransform(int days)
        {

        }
    }
}
