using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml;
using WsFederation.Messages;

namespace WsFederation
{
    public class WsFedAuthenticationHandler : AuthenticationHandler<WsFedAuthenticationOptions>
    {
        private static Dictionary<string, string> _mappingWifToOpenIdClaims = new Dictionary<string, string>
        {
            {
                ClaimTypes.Gender, "gender"
            },
            {
                ClaimTypes.GivenName, "given_name"
            },
            {
                ClaimTypes.Name, "name"
            },
            {
                ClaimTypes.Role, "role"
            },
            {
                ClaimTypes.DateOfBirth, "birthdate"
            },
            {
                ClaimTypes.Email, "email"
            },
            {
                ClaimTypes.MobilePhone, "phone_number"
            },
            {
                ClaimTypes.HomePhone, "phone_number_verified"
            },
            {
                ClaimTypes.Webpage, "website"
            }
        };

        public override async Task<bool> HandleRequestAsync()
        {
            if (Options.CallbackPath == Request.Path)
            {
                return await HandleRemoveCallbackAsync();
            }

            return false;
        }

        protected override async Task<bool> HandleUnauthorizedAsync(ChallengeContext context)
        {
            var returnUrl = BuildRedirectUri(Options.CallbackPath);
            var signInResponse = new SignInRequestMessage(new Uri(Options.IdPEndpoint), Options.Realm, returnUrl);
            Response.Redirect(signInResponse.RequestUrl);
            return true;
        }
        
        protected virtual async Task<bool> HandleRemoveCallbackAsync()
        {
            try
            {
                if (string.Equals(Request.Method, "POST", StringComparison.OrdinalIgnoreCase)
              && !string.IsNullOrWhiteSpace(Request.ContentType)
              // May have media/type; charset=utf-8, allow partial match.
              && Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase)
              && Request.Body.CanRead)
                {
                    if (!Request.Body.CanSeek)
                    {
                        // Buffer in case this body was not meant for us.
                        var memoryStream = new MemoryStream();
                        await Request.Body.CopyToAsync(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        Request.Body = memoryStream;
                    }

                    var form = await Request.ReadFormAsync();
                    var collection = new Dictionary<string, StringValues>();
                    foreach (var tuple in form)
                    {
                        collection.Add(tuple.Key, tuple.Value);
                    }

                    var uri = new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}");
                    var wsFederationMessage = WSFederationMessage.CreateFromNameValueCollection(uri, collection);
                    var xml = wsFederationMessage.GetParameter("wresult");

                    var document = new XmlDocument();
                    document.LoadXml(xml);

                    var nsMan = new XmlNamespaceManager(document.NameTable);
                    // Parse SAML2 response
                    nsMan.AddNamespace("trust", "http://docs.oasis-open.org/ws-sx/ws-trust/200512");
                    nsMan.AddNamespace("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");
                    var parentNodes = "trust:RequestSecurityTokenResponseCollection/trust:RequestSecurityTokenResponse/trust:RequestedSecurityToken/";
                    var assertionNode = document.SelectSingleNode(parentNodes + "saml2:EncryptedAssertion", nsMan);
                    if (assertionNode == null)
                    {
                        assertionNode = document.SelectSingleNode(parentNodes + "saml2:Assertion", nsMan);
                    }

                    // Parse SAML response
                    if (assertionNode == null)
                    {
                        nsMan = new XmlNamespaceManager(document.NameTable);
                        nsMan.AddNamespace("saml", "urn:oasis:names:tc:SAML:1.0:assertion");
                        nsMan.AddNamespace("t", "http://schemas.xmlsoap.org/ws/2005/02/trust");
                        parentNodes = "t:RequestSecurityTokenResponse/t:RequestedSecurityToken/";
                        assertionNode = document.SelectSingleNode(parentNodes + "saml:EncryptedAssertion", nsMan);
                        if (assertionNode == null)
                        {
                            assertionNode = document.SelectSingleNode(parentNodes + "saml:Assertion", nsMan);
                        }
                    }

                    if (assertionNode == null)
                    {
                        return false;
                    }

                    List<Claim> claims = null;
                    if (Options.GetClaimsCallback != null)
                    {
                        claims = Options.GetClaimsCallback(assertionNode.ChildNodes);
                    }
                    else
                    {
                        claims = new List<Claim>();
                        if (Options.Events.OnClaimsReceived != null)
                        {
                            claims = Options.Events.OnClaimsReceived(assertionNode);
                        }
                        else
                        {
                            foreach (XmlNode child in assertionNode.ChildNodes)
                            {
                                if (child.Name == "saml2:Subject" || child.Name == "saml:Subject")
                                {
                                    claims.Add(new Claim("sub", child.InnerText));
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
                                            }
                                        }

                                        claims.Add(ToClaim(id, attribute.InnerText));
                                    }
                                }
                            }
                        }
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, Options.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(claimsIdentity);
                    var ticket = new AuthenticationTicket(principal,
                        new AuthenticationProperties(),
                        Options.AuthenticationScheme);
                    await Context.Authentication.SignInAsync(
                        Options.SignInScheme,
                        principal,
                        new AuthenticationProperties());

                    var redirectPath = Options.RedirectPath;
                    if (string.IsNullOrWhiteSpace(redirectPath))
                    {
                        redirectPath = "/";
                    }

                    var redirectUrl = BuildRedirectUri(redirectPath);
                    Response.Redirect(redirectUrl);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (PriorHandler != null)
            {
                var authenticateContext = new AuthenticateContext(Options.SignInScheme);
                await PriorHandler.AuthenticateAsync(authenticateContext);
                if (authenticateContext.Accepted)
                {
                    if (authenticateContext.Error != null)
                    {
                        return AuthenticateResult.Fail(authenticateContext.Error);
                    }

                    if (authenticateContext.Principal != null)
                    {
                        return AuthenticateResult.Success(new AuthenticationTicket(authenticateContext.Principal,
                            new AuthenticationProperties(authenticateContext.Properties), Options.AuthenticationScheme));
                    }

                    return AuthenticateResult.Fail("Not authenticated");
                }

            }

            return AuthenticateResult.Fail("Remote authentication does not support authenticate");
        }

        private static Claim ToClaim(string key, string value)
        {
            if (!_mappingWifToOpenIdClaims.ContainsKey(key))
            {
                return new Claim(key, value);
            }

            return new Claim(_mappingWifToOpenIdClaims[key], value);
        }
    }
}
