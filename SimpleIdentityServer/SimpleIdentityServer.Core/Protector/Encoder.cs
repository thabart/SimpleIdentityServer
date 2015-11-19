using System;
using System.Text;
using SimpleIdentityServer.Core.Common.Extensions;

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
            var base64Bytes = code.Base64DecodeBytes();
            return new ASCIIEncoding().GetString(base64Bytes);
        }

        public string Encode(string code)
        {
            return Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(code));
        }
    }
}
