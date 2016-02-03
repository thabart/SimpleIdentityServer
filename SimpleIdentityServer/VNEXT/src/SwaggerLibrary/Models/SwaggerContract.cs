using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SwaggerLibrary.Models
{
    public class SwaggerContract
    {
        public string BasePath { get; set; }

        public string Host { get; set; }

        public string SwaggerVersion { get; set; }

        public List<SwaggerPath> Paths { get; set; }

        public static SwaggerContract FromJObject(JObject obj)
        {
            var basePath = string.Empty;
            JToken basePathToken;
            if (obj.TryGetValue("basePath", out basePathToken))
            {
                basePath = basePathToken.ToString();
            }

            var result = new SwaggerContract
            {
                BasePath = basePath,
                Host = obj["host"].ToString()
            };

            result.Paths = new List<SwaggerPath>();
            var pathArr = obj["paths"];
            foreach (var path in pathArr)
            {
                var record = SwaggerPath.FromJObject((JProperty)path);
                result.Paths.Add(record);
            }

            return result;
        }
    }
}
