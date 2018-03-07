using SimpleIdentityServer.Core.Jwt;
using System.Collections.Generic;
using System.Security.Claims;
using System.Xml;

namespace Parser
{
    public class Auth0WsFederationParser
    {
        private static Dictionary<string, string> _mappingWifToOpenIdClaims = new Dictionary<string, string>
        {
            {
                "nameidentifier", Constants.StandardResourceOwnerClaimNames.Subject
            }
        };

        public List<Claim> Process(XmlNode node)
        {
            var result = new List<Claim>();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "saml:AttributeStatement")
                {
                    foreach (XmlNode attribute in child.ChildNodes)
                    {
                        var id = string.Empty;
                        foreach (XmlAttribute metadata in attribute.Attributes)
                        {
                            if (metadata.Name == "AttributeName")
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
