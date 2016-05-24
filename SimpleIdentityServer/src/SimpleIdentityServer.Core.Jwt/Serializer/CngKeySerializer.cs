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

#if NET46
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;
#endif

namespace SimpleIdentityServer.Core.Jwt.Serializer
{
#if NET46
    public interface ICngKeySerializer
    {
        string SerializeCngKeyWithPrivateKey(CngKey toBeSerialized);

        CngKey DeserializeCngKeyWithPrivateKey(string serializedBlob);

        string SerializeCngKeyWithPublicKey(CngKey toBeSerialized);

        CngKey DeserializeCngKeyWithPublicKey(string serializedBlob);
    }

    public class CngKeySerializer : ICngKeySerializer
    {
#region Fields

        private readonly Dictionary<int, int> MappingSizeAndMagicNumberForPrivateKey = new Dictionary<int, int>
        {
            {
                32, 844317509
            },
            {
                48, 877871941
            },
            {
                66, 911426373
            }
        };

        public readonly Dictionary<int, int> MappingSizeAndMagicNumberForPublicKey = new Dictionary<int, int>
        {
            {
                32, 827540293
            },
            {
                48, 861094725
            },
            {
                66, 894649157
            }
        };

#endregion

#region Public methods

        public string SerializeCngKeyWithPrivateKey(CngKey toBeSerialized)
        {
            if (toBeSerialized == null)
            {
                throw new ArgumentNullException("toBeSerialized");
            }

            var privateBlob = toBeSerialized.Export(CngKeyBlobFormat.EccPrivateBlob);
            var lengthBytes = new[]
            {
                privateBlob[4],
                privateBlob[5],
                privateBlob[6],
                privateBlob[7]
            };

            // Part size in octet
            var partLength = BitConverter.ToInt32(lengthBytes, 0);
            if (!MappingSizeAndMagicNumberForPrivateKey.ContainsKey(partLength))
            {
                throw new InvalidOperationException(string.Format("the part length {0} is not correct", partLength));
            }

            var allPartsInBytes = ByteManipulator.RightmostBits(privateBlob, partLength * 8 * 3);
            var keyParts = ByteManipulator.Slice(allPartsInBytes, partLength);
            var cngKey = new CngKeySerialized
            {
                X = keyParts[0],
                Y = keyParts[1],
                D = keyParts[2]
            };

            var serializer = new XmlSerializer(typeof(CngKeySerialized));
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, cngKey);
                return writer.ToString();
            }
        }

        public CngKey DeserializeCngKeyWithPrivateKey(string serializedBlob)
        {
            if (string.IsNullOrWhiteSpace(serializedBlob))
            {
                throw new ArgumentNullException("serializedBlob");
            }

            var serializer = new XmlSerializer(typeof(CngKeySerialized));
            using (var reader = new StringReader(serializedBlob))
            {
                var cngKeySerialized = (CngKeySerialized)serializer.Deserialize(reader);
                if (cngKeySerialized.X.Length != cngKeySerialized.Y.Length ||
                    cngKeySerialized.Y.Length != cngKeySerialized.D.Length)
                {
                    throw new InvalidOperationException("the size of the different parts is not equal (x, y, d)");
                }

                var partLength = cngKeySerialized.X.Length;
                if (!MappingSizeAndMagicNumberForPrivateKey.ContainsKey(partLength))
                {
                    throw new InvalidOperationException(string.Format("the part length {0} is not valid", partLength));
                }

                var partSize = BitConverter.GetBytes(partLength);
                var magicNumber = MappingSizeAndMagicNumberForPrivateKey[partLength];
                var magicBytes = BitConverter.GetBytes(magicNumber);
                var blob = ByteManipulator.Concat(magicBytes, 
                    partSize, 
                    cngKeySerialized.X, 
                    cngKeySerialized.Y, 
                    cngKeySerialized.D);

                return CngKey.Import(blob, CngKeyBlobFormat.EccPrivateBlob);
            }
        }

        public string SerializeCngKeyWithPublicKey(CngKey toBeSerialized)
        {
            if (toBeSerialized == null)
            {
                throw new ArgumentNullException("toBeSerialized");
            }

            var publicBlob = toBeSerialized.Export(CngKeyBlobFormat.EccPublicBlob);
            var lengthBytes = new[]
            {
                publicBlob[4],
                publicBlob[5],
                publicBlob[6],
                publicBlob[7]
            };

            // Part size in octet
            var partLength = BitConverter.ToInt32(lengthBytes, 0);
            if (!MappingSizeAndMagicNumberForPublicKey.ContainsKey(partLength))
            {
                throw new InvalidOperationException(string.Format("the part length {0} is not correct", partLength));
            }

            var allPartsInBytes = ByteManipulator.RightmostBits(publicBlob, partLength * 8 * 2);
            var keyParts = ByteManipulator.Slice(allPartsInBytes, partLength);
            var cngKey = new CngKeySerialized
            {
                X = keyParts[0],
                Y = keyParts[1]
            };

            var serializer = new XmlSerializer(typeof(CngKeySerialized));
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, cngKey);
                return writer.ToString();
            }
        }

        public CngKey DeserializeCngKeyWithPublicKey(string serializedBlob)
        {
            if (string.IsNullOrWhiteSpace(serializedBlob))
            {
                throw new ArgumentNullException("serializedBlob");
            }


            var serializer = new XmlSerializer(typeof(CngKeySerialized));
            using (var reader = new StringReader(serializedBlob))
            {
                var cngKeySerialized = (CngKeySerialized)serializer.Deserialize(reader);
                if (cngKeySerialized.X.Length != cngKeySerialized.Y.Length)
                {
                    throw new InvalidOperationException("the size of the different parts is not equal (x, y)");
                }

                var partLength = cngKeySerialized.X.Length;
                if (!MappingSizeAndMagicNumberForPublicKey.ContainsKey(partLength))
                {
                    throw new InvalidOperationException(string.Format("the part length {0} is not valid", partLength));
                }

                var partSize = BitConverter.GetBytes(partLength);
                var magicNumber = MappingSizeAndMagicNumberForPublicKey[partLength];
                var magicBytes = BitConverter.GetBytes(magicNumber);
                var blob = ByteManipulator.Concat(magicBytes,
                    partSize,
                    cngKeySerialized.X,
                    cngKeySerialized.Y);

                return CngKey.Import(blob, CngKeyBlobFormat.EccPublicBlob);
            }
        }

#endregion
    }
#endif
}
