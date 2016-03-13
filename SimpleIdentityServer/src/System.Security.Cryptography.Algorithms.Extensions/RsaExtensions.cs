#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Diagnostics.Contracts;
using System.Text;

namespace System.Security.Cryptography
{
    public static class RsaExtensions
    {
        #region Public static methods

        public static string ToXmlString(this RSA rsa, bool includePrivateParameters)
        {
            // From the XMLDSIG spec, RFC 3075, Section 6.4.2,
            RSAParameters rsaParams = rsa.ExportParameters(includePrivateParameters);
            var builder = new StringBuilder();
            builder.Append("<RSAKeyValue>");
            // Add the modulus
            builder.Append("<Modulus>" + Convert.ToBase64String(rsaParams.Modulus) + "</Modulus>");
            // Add the exponent
            builder.Append("<Exponent>" + Convert.ToBase64String(rsaParams.Exponent) + "</Exponent>");
            if (includePrivateParameters)
            {
                // Add the private components 
                builder.Append("<P>" + Convert.ToBase64String(rsaParams.P) + "</P>");
                builder.Append("<Q>" + Convert.ToBase64String(rsaParams.Q) + "</Q>");
                builder.Append("<DP>" + Convert.ToBase64String(rsaParams.DP) + "</DP>");
                builder.Append("<DQ>" + Convert.ToBase64String(rsaParams.DQ) + "</DQ>");
                builder.Append("<InverseQ>" + Convert.ToBase64String(rsaParams.InverseQ) + "</InverseQ>");
                builder.Append("<D>" + Convert.ToBase64String(rsaParams.D) + "</D>");
            }

            builder.Append("</RSAKeyValue>");
            return builder.ToString();
        }

        public static void FromXmlString(this RSA rsa, string xmlString)
        {
            if (xmlString == null)
            {
                throw new ArgumentNullException(nameof(xmlString));
            }

            Contract.EndContractBlock();
            var rsaParams = new RSAParameters();
            var parser = new Parser(xmlString);
            var topElement = parser.GetTopElement();

            // Modulus is always present 
            var modulusString = topElement.SearchForTextOfLocalName("Modulus");
            if (modulusString == null)
            {
                throw new CryptographicException("Cryptography_InvalidFromXmlString");
            }

            rsaParams.Modulus = Convert.FromBase64String(Utils.DiscardWhiteSpaces(modulusString));

            // Exponent is always present 
            var exponentString = topElement.SearchForTextOfLocalName("Exponent");
            if (exponentString == null)
            {
                throw new CryptographicException("Cryptography_InvalidFromXmlString");
            }

            rsaParams.Exponent = Convert.FromBase64String(Utils.DiscardWhiteSpaces(exponentString));

            // P is optional
            var pString = topElement.SearchForTextOfLocalName("P");
            if (pString != null)
            {
                rsaParams.P = Convert.FromBase64String(Utils.DiscardWhiteSpaces(pString));
            }

            // Q is optional 
            var qString = topElement.SearchForTextOfLocalName("Q");
            if (qString != null)
            {
                rsaParams.Q = Convert.FromBase64String(Utils.DiscardWhiteSpaces(qString));
            }

            // DP is optional 
            var dpString = topElement.SearchForTextOfLocalName("DP");
            if (dpString != null)
            {
                rsaParams.DP = Convert.FromBase64String(Utils.DiscardWhiteSpaces(dpString));
            }

            // DQ is optional
            var dqString = topElement.SearchForTextOfLocalName("DQ");
            if (dqString != null)
            {
                rsaParams.DQ = Convert.FromBase64String(Utils.DiscardWhiteSpaces(dqString));
            }

            // InverseQ is optional
            var inverseQString = topElement.SearchForTextOfLocalName("InverseQ");
            if (inverseQString != null) rsaParams.InverseQ = Convert.FromBase64String(Utils.DiscardWhiteSpaces(inverseQString));

            // D is optional 
            var dString = topElement.SearchForTextOfLocalName("D");
            if (dString != null)
            {
                rsaParams.D = Convert.FromBase64String(Utils.DiscardWhiteSpaces(dString));
            }

            rsa.ImportParameters(rsaParams);
        }

        #endregion
    }
}
