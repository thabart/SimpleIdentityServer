using System;

namespace SimpleIdentityServer.Core.Jwt
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
    /// Identifies the operation(s) that the key is itended to be user for
    /// </summary>
    public enum KeyOperations
    {
        /// <summary>
        /// Compute digital signature or MAC
        /// </summary>
        Sign,
        /// <summary>
        /// Verify digital signature or MAC
        /// </summary>
        Verify,
        /// <summary>
        /// Encrypt content
        /// </summary>
        Encrypt,
        /// <summary>
        /// Decrypt content and validate decryption if applicable
        /// </summary>
        Decrypt,
        /// <summary>
        /// Encrypt key
        /// </summary>
        WrapKey,
        /// <summary>
        /// Decrypt key and validate encryption if applicable
        /// </summary>
        UnWrapKey,
        /// <summary>
        /// Derive key
        /// </summary>
        DeriveKey,
        /// <summary>
        /// Derive bits not to be used as a key
        /// </summary>
        DeriveBits
    }

    /// <summary>
    /// Algorithms used to create JWS
    /// Links to documentation : https://tools.ietf.org/html/rfc7518#page-6
    /// </summary>
    public enum JwsAlg
    {
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
        none
    }

    /// <summary>
    /// Algorithms used to create JWE
    /// Link to documentation : https://tools.ietf.org/html/rfc7518#page-12
    /// </summary>
    public enum JweAlg
    {
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
    }

    /// <summary>
    /// Encryptions algorithms for JWE : https://tools.ietf.org/html/rfc7518#page-22
    /// </summary>
    public enum JweEnc
    {
        A128CBC_HS256, //AES_128_CBC_HMAC_SHA_256 authenticated encryption using a 256 bit key. : documentation : https://tools.ietf.org/html/draft-ietf-jose-json-web-encryption-40#appendix-B
        A192CBC_HS384,
        A256CBC_HS512,
        A128GCM,
        A192GCM,
        A256GCM
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

    /// <summary>
    /// Definition of a JSON Web Key (JWK)
    /// It's a JSON data structure that represents a cryptographic key
    /// </summary>
    public class JsonWebKey
    {
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
        /// Gets or sets the operation(s) that the key is intended to be user for.
        /// </summary>
        public KeyOperations[] KeyOps { get; set; }

        /// <summary>
        /// Gets or sets the algorithm intended for use with the key
        /// </summary>
        public AllAlg Alg { get; set; }

        /// <summary>
        /// Gets or sets the KID (key id). 
        /// </summary>
        public string Kid { get; set; }

        /// <summary>
        /// Gets or sets the X5U. It's a URI that refers to a resource for an X509 public key certificate or certificate chain.
        /// </summary>
        public Uri X5u { get; set; }

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
    }
}
