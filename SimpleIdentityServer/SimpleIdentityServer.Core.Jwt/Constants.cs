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
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdentityServer.Core.Jwt
{
    public class Constants
    {
        public static class StandardClaimNames
        {
            public static string Issuer = "iss";

            public static string Audiences = "aud";

            public static string ExpirationTime = "exp";

            public static string Iat = "iat";

            public static string AuthenticationTime = "auth_time";

            public static string Nonce = "nonce";

            public static string Acr = "acr";

            public static string Amr = "amr";

            public static string Azp = "azp";

            /// <summary>
            /// Unique identifier of the JWT.
            /// </summary>
            public static string Jti = "jti";
        }

        public static class StandardResourceOwnerClaimNames
        {
            public static string Subject = "sub";

            public static string Name = "name";

            public static string GivenName = "given_name";

            public static string FamilyName = "family_name";

            public static string MiddleName = "middle_name";

            public static string NickName = "nickname";

            public static string PreferredUserName = "preferred_username";

            public static string Profile = "profile";

            public static string Picture = "picture";

            public static string WebSite = "website";

            public static string Email = "email";

            public static string EmailVerified = "email_verified";

            public static string Gender = "gender";

            public static string BirthDate = "birthdate";

            public static string ZoneInfo = "zoneinfo";

            public static string Locale = "locale";

            public static string PhoneNumber = "phone_number";

            public static string PhoneNumberVerified = "phone_number_verified";

            public static string Address = "address";

            public static string UpdatedAt = "updated_at";

            // Check where this claims is defined.
            public static string Role = "role";
        }

        public static List<string> AllStandardResourceOwnerClaimNames = new List<string>
        {
            StandardResourceOwnerClaimNames.Subject,
            StandardResourceOwnerClaimNames.Address,
            StandardResourceOwnerClaimNames.BirthDate,
            StandardResourceOwnerClaimNames.Email,
            StandardResourceOwnerClaimNames.EmailVerified,
            StandardResourceOwnerClaimNames.FamilyName,
            StandardResourceOwnerClaimNames.Gender,
            StandardResourceOwnerClaimNames.GivenName,
            StandardResourceOwnerClaimNames.Locale,
            StandardResourceOwnerClaimNames.MiddleName,
            StandardResourceOwnerClaimNames.Name,
            StandardResourceOwnerClaimNames.NickName,
            StandardResourceOwnerClaimNames.PhoneNumber,
            StandardResourceOwnerClaimNames.PhoneNumberVerified,
            StandardResourceOwnerClaimNames.Picture,
            StandardResourceOwnerClaimNames.PreferredUserName,
            StandardResourceOwnerClaimNames.Profile,
            StandardResourceOwnerClaimNames.Role,
            StandardResourceOwnerClaimNames.Subject,
            StandardResourceOwnerClaimNames.UpdatedAt,
            StandardResourceOwnerClaimNames.WebSite,
            StandardResourceOwnerClaimNames.ZoneInfo
        };

        public static List<string> AllStandardClaimNames = new List<string>
        {
            StandardClaimNames.Acr,
            StandardClaimNames.Amr,
            StandardClaimNames.Audiences,
            StandardClaimNames.AuthenticationTime,
            StandardClaimNames.Azp,
            StandardClaimNames.ExpirationTime,
            StandardClaimNames.Iat,
            StandardClaimNames.Issuer,
            StandardClaimNames.Jti,
            StandardClaimNames.Nonce
        };

        public static readonly Dictionary<string, string> MapWifClaimsToOpenIdClaims = new Dictionary<string, string>
        {
            {
                ClaimTypes.Name, StandardResourceOwnerClaimNames.Name
            },
            {
                ClaimTypes.GivenName, StandardResourceOwnerClaimNames.GivenName
            },
            {
                ClaimTypes.Webpage, StandardResourceOwnerClaimNames.WebSite
            },
            {
                ClaimTypes.Email, StandardResourceOwnerClaimNames.Email
            },
            {
                ClaimTypes.Gender, StandardResourceOwnerClaimNames.Gender
            },
            {
                ClaimTypes.DateOfBirth, StandardResourceOwnerClaimNames.BirthDate
            },
            {
                ClaimTypes.Locality, StandardResourceOwnerClaimNames.Locale
            },
            {
                ClaimTypes.HomePhone, StandardResourceOwnerClaimNames.PhoneNumber
            },
            {
                ClaimTypes.MobilePhone, StandardResourceOwnerClaimNames.PhoneNumberVerified
            },
            {
                ClaimTypes.StreetAddress, StandardResourceOwnerClaimNames.Address
            },
            {
                ClaimTypes.Role, StandardResourceOwnerClaimNames.Role
            }
        };

        public static readonly Dictionary<string, JwsAlg> MappingNameToJwsAlg = new Dictionary<string, JwsAlg>
        {
            {
                JwsAlgNames.HS256, JwsAlg.HS256
            },
            {
                JwsAlgNames.HS384, JwsAlg.HS384
            },
            {
                JwsAlgNames.HS512, JwsAlg.HS512
            },
            {
                JwsAlgNames.RS256, JwsAlg.RS256
            },
            {
                JwsAlgNames.RS384, JwsAlg.RS384
            },
            {
                JwsAlgNames.RS512, JwsAlg.RS512
            },
            {
                JwsAlgNames.ES256, JwsAlg.ES256
            },
            {
                JwsAlgNames.ES384, JwsAlg.ES384
            },
            {
                JwsAlgNames.ES512, JwsAlg.ES512
            },
            {
                JwsAlgNames.PS256, JwsAlg.PS256
            },
            {
                JwsAlgNames.PS384, JwsAlg.PS384
            },
            {
                JwsAlgNames.PS512, JwsAlg.PS512
            },
            {
                JwsAlgNames.NONE, JwsAlg.none
            }
        };

        public static Dictionary<string, AllAlg> MappingNameToAllAlgEnum = new Dictionary<string, AllAlg>
        {
            {
                JwsAlgNames.HS256, AllAlg.HS256
            },
            {
                JwsAlgNames.HS384, AllAlg.HS384
            },
            {
                JwsAlgNames.HS512, AllAlg.HS512
            },
            {
                JwsAlgNames.RS256, AllAlg.RS256
            },
            {
                JwsAlgNames.RS384, AllAlg.RS384
            },
            {
                JwsAlgNames.RS512, AllAlg.RS512
            },
            {
                JwsAlgNames.ES256, AllAlg.ES256
            },
            {
                JwsAlgNames.ES384, AllAlg.ES384
            },
            {
                JwsAlgNames.ES512, AllAlg.ES512
            },
            {
                JwsAlgNames.PS256, AllAlg.PS256
            },
            {
                JwsAlgNames.PS384, AllAlg.PS384
            },
            {
                JwsAlgNames.PS512, AllAlg.PS512
            },
            {
                JwsAlgNames.NONE, AllAlg.none
            },            
            {
                JweAlgNames.RSA1_5, AllAlg.RSA1_5
            },
            {
                JweAlgNames.RSA_OAEP, AllAlg.RSA_OAEP
            },
            {
                JweAlgNames.RSA_OAEP_256, AllAlg.RSA_OAEP_256
            },
            {
                JweAlgNames.A128KW, AllAlg.A128KW
            },
            {
                JweAlgNames.A128GCMKW, AllAlg.A128GCMKW
            },
            {
                JweAlgNames.A192GCMKW, AllAlg.A192GCMKW
            },
            {
                JweAlgNames.A192KW, AllAlg.A192KW
            },
            {
                JweAlgNames.A256GCMKW, AllAlg.A256GCMKW
            },
            {
                JweAlgNames.A256KW, AllAlg.A256KW
            },
            {
                JweAlgNames.DIR, AllAlg.DIR
            },
            {
                JweAlgNames.ECDH_ESA_128KW, AllAlg.ECDH_ESA_128KW
            },
            {
                JweAlgNames.ECDH_ESA_192KW, AllAlg.ECDH_ESA_192KW
            },
            {
                JweAlgNames.ECDH_ESA_256_KW, AllAlg.ECDH_ESA_256_KW
            },
            {
                JweAlgNames.PBES2_HS256_A128KW, AllAlg.PBES2_HS256_A128KW
            },
            {
                JweAlgNames.PBES2_HS384_A192KW, AllAlg.PBES2_HS384_A192KW
            },
            {
                JweAlgNames.PBES2_HS512_A256KW, AllAlg.PBES2_HS512_A256KW
            }
        };
        public static readonly Dictionary<string, JweAlg> MappingNameToJweAlgEnum = new Dictionary<string, JweAlg>
        {
            {
                JweAlgNames.RSA1_5, JweAlg.RSA1_5
            },
            {
                JweAlgNames.RSA_OAEP, JweAlg.RSA_OAEP
            },
            {
                JweAlgNames.RSA_OAEP_256, JweAlg.RSA_OAEP_256
            },
            {
                JweAlgNames.A128KW, JweAlg.A128KW
            },
            {
                JweAlgNames.A128GCMKW, JweAlg.A128GCMKW
            },
            {
                JweAlgNames.A192GCMKW, JweAlg.A192GCMKW
            },
            {
                JweAlgNames.A192KW, JweAlg.A192KW
            },
            {
                JweAlgNames.A256GCMKW, JweAlg.A256GCMKW
            },
            {
                JweAlgNames.A256KW, JweAlg.A256KW
            },
            {
                JweAlgNames.DIR, JweAlg.DIR
            },
            {
                JweAlgNames.ECDH_ESA_128KW, JweAlg.ECDH_ESA_128KW
            },
            {
                JweAlgNames.ECDH_ESA_192KW, JweAlg.ECDH_ESA_192KW
            },
            {
                JweAlgNames.ECDH_ESA_256_KW, JweAlg.ECDH_ESA_256_KW
            },
            {
                JweAlgNames.PBES2_HS256_A128KW, JweAlg.PBES2_HS256_A128KW
            },
            {
                JweAlgNames.PBES2_HS384_A192KW, JweAlg.PBES2_HS384_A192KW
            },
            {
                JweAlgNames.PBES2_HS512_A256KW, JweAlg.PBES2_HS512_A256KW
            }
        };

        public static readonly Dictionary<string, JweEnc> MappingNameToJweEncEnum = new Dictionary<string, JweEnc>
        {
            {
                JweEncNames.A128CBC_HS256, JweEnc.A128CBC_HS256
            },
            {
                JweEncNames.A192CBC_HS384, JweEnc.A192CBC_HS384
            },
            {
                JweEncNames.A256CBC_HS512, JweEnc.A256CBC_HS512
            }
        };

        public static class JweEncNames 
        {
            public static string A128CBC_HS256 = "A128CBC-HS256";

            public static string A192CBC_HS384 = "A192CBC-HS384";

            public static string A256CBC_HS512 = "A256CBC-HS512";
        }

        public static class JweAlgNames
        {
            public static string RSA1_5 = "RSA1_5";

            public static string RSA_OAEP = "RSA-OAEP";

            public static string RSA_OAEP_256 = "RSA-OAEP-256";

            public static string A128KW = "A128KW";

            public static string A192KW = "A192KW";

            public static string A256KW = "A256KW";

            public static string DIR = "dir";

            public static string ECDH_ES = "ECDH-ES";

            public static string ECDH_ESA_128KW = "ECDH-ES+A128KW";

            public static string ECDH_ESA_192KW = "ECDH-ES+A192KW";

            public static string ECDH_ESA_256_KW = "ECDH-ES+A256KW";

            public static string A128GCMKW = "A128GCMKW";

            public static string A192GCMKW = "A192GCMKW";

            public static string A256GCMKW = "A256GCMKW";

            public static string PBES2_HS256_A128KW = "PBES2-HS256+A128KW";

            public static string PBES2_HS384_A192KW = "PBES2-HS384+A192KW";

            public static string PBES2_HS512_A256KW = "PBES2-HS512+A256KW";
        }

        public static class JwsAlgNames
        {
            public static string HS256 = "HS256";

            public static string HS384 = "HS384";

            public static string HS512 = "HS512";

            public static string RS256 = "RS256";

            public static string RS384 = "RS384";

            public static string RS512 = "RS512";

            public static string ES256 = "ES256";

            public static string ES384 = "ES384";

            public static string ES512 = "ES512";

            public static string PS256 = "PS256";

            public static string PS384 = "PS384";

            public static string PS512 = "PS512";

            public static string NONE = "none";
        }

        public static class JsonWebKeyParameterNames
        {
            public static string KeyTypeName = "kty";

            public static string UseName = "use";

            public static string KeyOperationsName = "key_ops";

            public static string AlgorithmName = "alg";

            public static string KeyIdentifierName = "kid";

            public static string X5Url = "x5u";

            public static string X5CertificateChain = "x5c";

            public static string X5ThumbPrint = "x5t";

            public static string X5Sha256ThumbPrint = "x5t#S256";

            public static class RsaKey
            {
                public static string ModulusName = "n";

                public static string ExponentName = "e";
            }
        }

        public static Dictionary<KeyType, string> MappingKeyTypeEnumToName = new Dictionary<KeyType, string>
        {
            {
                KeyType.RSA, "RSA"
            },
            {
                KeyType.EC, "EC"
            },
            {
                KeyType.oct, "oct"
            }
        };

        public static Dictionary<Use, string> MappingUseEnumerationToName  = new Dictionary<Use, string>
        {
            {
                Use.Sig, "sig"
            },
            {
                Use.Enc, "enc"
            }
        };

        public static Dictionary<KeyOperations, string> MappingKeyOperationToName = new Dictionary<KeyOperations, string>
        {
            {
                KeyOperations.Sign, "sign"
            },
            {
                KeyOperations.Verify, "verify"
            },
            {
                KeyOperations.Encrypt, "encrypt"
            },
            {
                KeyOperations.Decrypt, "decrypt"
            },
            {
                KeyOperations.WrapKey, "wrapKey"
            },
            {
                KeyOperations.UnWrapKey, "unwrapKey"
            },
            {
                KeyOperations.DeriveKey, "deriveKey"
            },
            {
                KeyOperations.DeriveBits, "deriveBits"
            }
        };
    }
}
