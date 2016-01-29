using System.Web.Script.Serialization;

using SimpleIdentityServer.Core.Protector;

namespace SimpleIdentityServer.Api.Tests.Common.Fakes
{
    /// <summary>
    /// Fake the protector because it taking some time to execute the UTs.
    /// </summary>
    public class FakeProtector : IProtector
    {
        public string Encrypt(object obj)
        {
            var json = new JavaScriptSerializer().Serialize(obj);
            return json;
        }

        public T Decrypt<T>(string encryptedString)
        {
            return new JavaScriptSerializer().Deserialize<T>(encryptedString);
        }
    }
}
