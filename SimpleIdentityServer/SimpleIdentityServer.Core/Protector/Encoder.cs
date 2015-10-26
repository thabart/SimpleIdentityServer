using System;
using System.Text;

namespace SimpleIdentityServer.Core.Protector
{
    public interface IEncoder
    {
        string Decode(string code);

        string Encode(string code);
    }

    public class Encoder : IEncoder
    {
        public string Decode(string code)
        {
            var base64Bytes = Convert.FromBase64String(code);
            return new ASCIIEncoding().GetString(base64Bytes);
        }

        public string Encode(string code)
        {
            return Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(code));
        }
    }
}
