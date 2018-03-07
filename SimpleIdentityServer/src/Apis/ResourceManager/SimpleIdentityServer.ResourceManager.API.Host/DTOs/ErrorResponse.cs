using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.ResourceManager.API.Host.DTOs
{
    internal sealed class ErrorResponse
    {
        public ErrorResponse(params string[] parameters)
        {
            Parameters = parameters;
        }

        public IEnumerable<string> Parameters { get; private set; }

        public JObject GetJson()
        {
            var result = new JObject();
            if (Parameters != null && Parameters.Any())
            {
                if (Parameters.Count() == 1)
                {
                    result.Add(Constants.ErrorDtoNames.Error, Parameters.First());
                }
                else
                {
                    result.Add(Constants.ErrorDtoNames.Error, new JArray(Parameters));
                }
            }

            return result;
        }
    }
}
