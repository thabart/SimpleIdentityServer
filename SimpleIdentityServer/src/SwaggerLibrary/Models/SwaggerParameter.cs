using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SwaggerLibrary.Models
{
    public class SwaggerParameter
    {
        private static Dictionary<string, SwaggerParameterInEnum> ParameterInEnums = new Dictionary<string, SwaggerParameterInEnum>
        {
            { "QUERY", SwaggerParameterInEnum.Query },
            { "BODY", SwaggerParameterInEnum.Body },
            { "PATH", SwaggerParameterInEnum.Path }
        };

        public string Name { get; set; }

        public SwaggerParameterInEnum In { get; set; }

        public string Type { get; set; }

        public static SwaggerParameter FromJObject(JObject obj)
        {
            if (!ParameterInEnums.ContainsKey(obj["in"].ToString().ToUpperInvariant()))
            {
                throw new NotSupportedException(string.Format("The swagger parameter type {0} is not recognized", obj["in"].ToString()));
            }

            return new SwaggerParameter
            {
                In = ParameterInEnums[obj["in"].ToString().ToUpperInvariant()],
                Name = obj["name"].ToString()
            };
        }
    }
}
