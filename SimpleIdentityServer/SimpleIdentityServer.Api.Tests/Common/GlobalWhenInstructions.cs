using System.Threading;
using TechTalk.SpecFlow;

namespace SimpleIdentityServer.Api.Tests.Common
{
    [Binding]
    public class GlobalWhenInstructions
    {
        [When("waiting for (.*) seconds")]
        public void WhenWaitingForSeconds(int milliSeconds)
        {
            Thread.Sleep(milliSeconds);
        }
    }
}
