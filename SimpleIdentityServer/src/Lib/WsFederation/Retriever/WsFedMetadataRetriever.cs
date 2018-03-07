using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml;

namespace WsFederation.Retriever
{
    public static class WsFedMetadataRetriever
    {
        #region Fields

        private static readonly XmlReaderSettings SafeSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit };

        #endregion

        public static IssuerFederationData GetFederationMetataData(string metadataEndpoint, HttpClient metadataRequest)
        {
            string issuer = string.Empty;
            string passiveTokenEndpoint = string.Empty;
            // TODO: Any way to make this async?
            using (var metadataResponse = metadataRequest.GetAsync(metadataEndpoint).Result)
            {
                metadataResponse.EnsureSuccessStatusCode();
                var content = metadataResponse.Content.ReadAsStringAsync().Result;

                /*
            using (XmlReader metaDataReader = XmlReader.Create(metadataStream, SafeSettings))
            {
                var serializer = new MetadataSerializer { CertificateValidationMode = X509CertificateValidationMode.None };

                MetadataBase metadata = serializer.ReadMetadata(metaDataReader);
                var entityDescriptor = (EntityDescriptor)metadata;

                if (!string.IsNullOrWhiteSpace(entityDescriptor.EntityId.Id))
                {
                    issuer = entityDescriptor.EntityId.Id;
                }

                SecurityTokenServiceDescriptor stsd = entityDescriptor.RoleDescriptors.OfType<SecurityTokenServiceDescriptor>().First();
                if (stsd == null)
                {
                    throw new InvalidOperationException(Resources.Exception_MissingDescriptor);
                }

                passiveTokenEndpoint = stsd.PassiveRequestorEndpoints.First().Uri.AbsoluteUri;

                IEnumerable<X509RawDataKeyIdentifierClause> x509DataClauses =
                    stsd.Keys.Where(key => key.KeyInfo != null
                        && (key.Use == KeyType.Signing || key.Use == KeyType.Unspecified))
                            .Select(key => key.KeyInfo.OfType<X509RawDataKeyIdentifierClause>().First());

                signingTokens.AddRange(x509DataClauses.Select(token => new X509SecurityToken(new X509Certificate2(token.GetX509RawData()))));
            }
            }
            return new IssuerFederationData { IssuerSigningTokens = signingTokens, PassiveTokenEndpoint = passiveTokenEndpoint, TokenIssuerName = issuer };
            */
            return null;
            }
        }
    }
}
