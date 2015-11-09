using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SimpleIdentityServer.Core.Jwt.Signature
{
    public interface ICreateJwsSignature
    {
        string SignWithRsa(
            JwsAlg algorithm,
            string serializedKeys,
            string combinedJwsNotSigned);
    }

    public class CreateJwsSignature : ICreateJwsSignature
    {
        private readonly Dictionary<JwsAlg, string> _mappingJwsAlgorithmToRsaHashingAlgorithms = new Dictionary<JwsAlg, string>
        {
            {
                JwsAlg.RS256, "SHA256"
            },
            {
                JwsAlg.RS384, "SHA384"
            },
            {
                JwsAlg.RS512, "SHA512"
            }
        }; 

        public string SignWithRsa(
            JwsAlg algorithm, 
            string serializedKeys,
            string combinedJwsNotSigned)
        {
            if (!_mappingJwsAlgorithmToRsaHashingAlgorithms.ContainsKey(algorithm))
            {
                return null;
            }

            using (var rsa = new RSACryptoServiceProvider())
            {
                var bytesToBeSigned = ASCIIEncoding.ASCII.GetBytes(combinedJwsNotSigned);
                rsa.FromXmlString(serializedKeys);
                var byteToBeConverted = rsa.SignData(bytesToBeSigned, "");
                return Convert.ToBase64String(byteToBeConverted);
            }
        }

        public string SignWithEllipseCurve(JwsAlg algorithm)
        {
            return string.Empty;
        }
    }
}
