using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SwaggerLibrary.Models
{
    public class SwaggerPath
    {
        public string Path { get; set; }

        public List<SwaggerOperation> Operations { get; set; }

        public static SwaggerPath FromJObject(JProperty obj)
        {
            var result = new SwaggerPath
            {
                Path = obj.Name
            };

            var operations = obj.Value;
            result.Operations = new List<SwaggerOperation>();
            foreach (var operation in operations)
            {
                var record = SwaggerOperation.FromJObject((JProperty)operation);
                result.Operations.Add(record);
            }
            
            return result;
        }
    }
}
