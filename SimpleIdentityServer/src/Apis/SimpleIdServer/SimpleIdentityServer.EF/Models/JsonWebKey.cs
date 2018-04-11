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

using SimpleIdentityServer.Core.Jwt;

namespace SimpleIdentityServer.EF.Models
{
    /// <summary>
    /// Key types
    /// </summary>
    public enum KeyType
    {
        /// <summary>
        /// Ellipse Curve
        /// </summary>
        EC,
        /// <summary>
        /// RSA
        /// </summary>
        RSA,
        /// <summary>
        /// Octet sequence (used to represent symmetric keys)
        /// </summary>
        oct
    }

    /// <summary>
    /// Identifies the itended use of the Public Key.
    /// </summary>
    public enum Use
    {
        /// <summary>
        /// Signature
        /// </summary>
        Sig,
        /// <summary>
        /// Encryption
        /// </summary>
        Enc
    }

    /// <summary>
    /// Algorithm for JWS & JWE
    /// </summary>
    public enum AllAlg
    {
        #region JWS ALGORITHMS
        HS256,
        HS384,
        HS512,
        RS256,
        RS384,
        RS512,
        ES256,
        ES384,
        ES512,
        PS256,
        PS384,
        PS512,
        none,
        #endregion

        #region JWE ALGORITHMS
        RSA1_5,
        RSA_OAEP,
        RSA_OAEP_256,
        A128KW,
        A192KW,
        A256KW,
        DIR,
        ECDH_ES,
        ECDH_ESA_128KW,
        ECDH_ESA_192KW,
        ECDH_ESA_256_KW,
        A128GCMKW,
        A192GCMKW,
        A256GCMKW,
        PBES2_HS256_A128KW,
        PBES2_HS384_A192KW,
        PBES2_HS512_A256KW
        #endregion
    }

    public class JsonWebKey
    {
        /// <summary>
        /// Gets or sets the KID (key id). 
        /// </summary>
        public string Kid { get; set; }

        /// <summary>
        /// Gets or sets the cryptographic algorithm family used with the key.
        /// </summary>
        public KeyType Kty { get; set; }

        /// <summary>
        /// Gets or sets the intended use of the public key.
        /// Employed to indicate whether a public key is used for encrypting data or verifying the signature on data.
        /// </summary>
        public Use Use { get; set; }

        /// <summary>
        /// Gets or sets the concatenated list of key operations separated by ','
        /// </summary>
        public string KeyOps { get; set; }

        /// <summary>
        /// Gets or sets the algorithm intended for use with the key
        /// </summary>
        public AllAlg Alg { get; set; }
        
        /// <summary>
        /// Gets or sets the X5U. It's a URI that refers to a resource for an X509 public key certificate or certificate chain.
        /// </summary>
        public string X5u { get; set; }
        
        /// <summary>
        /// Gets or sets the X5T. Is a base64url encoded SHA-1 thumbprint of the DER encoding of an X509 certificate.
        /// </summary>
        public string X5t { get; set; }

        /// <summary>
        /// Gets or sets the X5T#S256. Is a base64url encoded SHA-256 thumbprint.
        /// </summary>
        public string X5tS256 { get; set; }

        /// <summary>
        /// Gets or sets the serialized key in XML
        /// </summary>
        public string SerializedKey { get; set; }

        public string ClientId { get; set; }

        public Client Client { get; set; }
    }
}
