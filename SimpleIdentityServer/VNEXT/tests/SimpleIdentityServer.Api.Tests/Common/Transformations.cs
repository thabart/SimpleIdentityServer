using System.Collections.Generic;
using System.Linq;

using TechTalk.SpecFlow;

namespace SimpleIdentityServer.Api.Tests.Common
{
    [Binding]
    public class Transformations
    {
        [StepArgumentTransformation(@"((?:.+,\s+)*(?:.+))")]
        public List<string> TransformIntoArray(string value)
        {
            return value.Split(',').ToList();
        }
    }
}
