using System.Collections.Generic;

namespace SimpleIdentityServer.Core.Results
{
    public class Parameter
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }

    public class RedirectInstruction
    {
        public RedirectInstruction()
        {
            Parameters = new List<Parameter>();
        }

        public IList<Parameter> Parameters { get; private set; }

        public IdentityServerEndPoints Action { get; set; }

        public void AddParameter(string name, string value)
        {
            var record = new Parameter
            {
                Name = name,
                Value = value
            };

            Parameters.Add(record);
        }
    }
}
