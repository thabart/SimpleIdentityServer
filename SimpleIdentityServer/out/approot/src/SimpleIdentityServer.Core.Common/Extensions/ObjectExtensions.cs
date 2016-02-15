using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;

namespace SimpleIdentityServer.Core.Common.Extensions
{
    public static class ObjectExtensions
    {
        public static string SerializeWithDataContract(this object parameter)
        {
            var serializer = new DataContractJsonSerializer(parameter.GetType());
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, parameter);
                ms.Position = 0;
                var reader = new StreamReader(ms);
                return reader.ReadToEnd();
            }
        }

        public static T DeserializeWithDataContract<T>(this string serialized)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            var ms = new MemoryStream(Encoding.Unicode.GetBytes(serialized));
            var obj = serializer.ReadObject(ms);
            return (T)obj;
        }

        public static string SerializeWithJavascript(this object parameter)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(parameter);
        }

        public static T DeserializeWithJavascript<T>(this string parameter)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(parameter);
        }
    }
}
