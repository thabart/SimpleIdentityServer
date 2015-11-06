namespace SimpleIdentityServer.Core.Cryptography
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
    /// Algorithm for JWS & JWE
    /// </summary>
    public enum Alg
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
        RSA15,
        RSAOAEP,
        RSAOAEP256,
        A128KW,
        A192KW,
        A256KW,
        dir,
        ECDHES,
        ECDHESA128KW,
        ECDHESA192KW,
        ECDHESA256KW,
        A128GCMKW,
        A192GCMKW,
        A256GCMKW,
        PBES2HS256A128KW,
        PBES2HS384A192KW,
        PBES2HS512A256KW
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
        public Alg Alg { get; set; }

        // TODO : continue to implement the other parameters : https://tools.ietf.org/html/draft-ietf-jose-json-web-key-41#section-4.6
    }
}
