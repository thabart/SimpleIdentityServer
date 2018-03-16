using SimpleIdentityServer.Core.Jwt;
using System.Collections.Generic;
using System.Security.Claims;
using System.Xml;

namespace Parser
{
    public class EidClaimsParser
    {
        private static Dictionary<string, string> _mappingWifToOpenIdClaims = new Dictionary<string, string>
        {
            {
                ClaimTypes.Gender, Constants.StandardResourceOwnerClaimNames.Gender
            },
            {
                ClaimTypes.GivenName, Constants.StandardResourceOwnerClaimNames.GivenName
            },
            {
                ClaimTypes.Name, Constants.StandardResourceOwnerClaimNames.Name
            },
            {
                ClaimTypes.Role,Constants.StandardResourceOwnerClaimNames.Role
            },
            {
                ClaimTypes.DateOfBirth, Constants.StandardResourceOwnerClaimNames.BirthDate
            },
            {
                ClaimTypes.Email, Constants.StandardResourceOwnerClaimNames.Email
            },
            {
                ClaimTypes.MobilePhone, Constants.StandardResourceOwnerClaimNames.PhoneNumber
            },
            {
                ClaimTypes.HomePhone, Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified
            },
            {
                ClaimTypes.Webpage, Constants.StandardResourceOwnerClaimNames.WebSite
            }
        };

        public List<Claim> Process(XmlNode node)
        {
            var result = new List<Claim>();
            foreach(XmlNode child in node.ChildNodes)
            {
                if (child.Name == "saml2:Subject" || child.Name == "saml:Subject")
                {
                    result.Add(new Claim(Constants.StandardResourceOwnerClaimNames.Subject, child.InnerText));
                }

                if (child.Name == "saml2:AttributeStatement"
                    || child.Name == "saml:AttributeStatement")
                {
                    foreach (XmlNode attribute in child.ChildNodes)
                    {
                        var id = string.Empty;
                        foreach (XmlAttribute metadata in attribute.Attributes)
                        {
                            if (metadata.Name == "Name")
                            {
                                id = metadata.Value;
                                break;
                            }
                        }

                        if (_mappingWifToOpenIdClaims.ContainsKey(id))
                        {
                            id = _mappingWifToOpenIdClaims[id];
                        }

                        result.Add(new Claim(id, attribute.InnerText));
                    }
                }
            }

            return result;
        }
    }
}
