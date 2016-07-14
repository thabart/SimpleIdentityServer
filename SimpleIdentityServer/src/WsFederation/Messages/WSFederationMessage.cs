using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using WsFederation.Extensions;

namespace WsFederation.Messages
{
    public abstract class WSFederationMessage : FederationMessage
    {
        #region Properties

        /// <summary>
        /// Gets or sets the wctx parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// The value of the wctx parameter.
        /// </returns>
        public string Context
        {
            get
            {
                return GetParameter("wctx");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    RemoveParameter("wctx");
                }
                else
                {
                    SetParameter("wctx", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the wencoding parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// The value of the wencoding parameter.
        /// </returns>
        public string Encoding
        {
            get
            {
                return GetParameter("wencoding");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    RemoveParameter("wencoding");
                }
                else
                {
                    SetParameter("wencoding", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the wa parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// The value of the wa parameter.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">An attempt to set a value that is null or empty occurs.</exception>
        public string Action
        {
            get
            {
                return GetParameter("wa");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                SetParameter("wa", value);
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IdentityModel.Services.WSFederationMessage"/> class from the base URL to which the message applies and the action to be performed (the wa message parameter).
        /// </summary>
        /// <param name="baseUrl">The base URL to which the WS-Federation message applies. This is the URL without any query parameters. Sets the <see cref="P:System.IdentityModel.Services.FederationMessage.BaseUri"/> property.</param><param name="action">The wa parameter of the message. Specifies the action to be performed; for example “wsignin1.0” for a WS-Federation sign-in request. Sets the <see cref="P:System.IdentityModel.Services.WSFederationMessage.Action"/> property.</param><exception cref="T:System.ArgumentNullException"><paramref name="baseUri"/> is null.</exception><exception cref="T:System.ArgumentException"><paramref name="action"/> is null or an empty string.</exception><exception cref="T:System.IdentityModel.Services.WSFederationMessageException"><paramref name="baseUri"/> is not a valid, absolute URI.</exception>
        public WSFederationMessage(Uri baseUrl, string action)
          : base(baseUrl)
        {
            if (string.IsNullOrEmpty(action))
            {
                throw new ArgumentNullException(nameof(action));
            }

            Parameters["wa"] = action;
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Creates a WS-Federation message from the specified URI. The parameters are assumed to be specified in the query string.
        /// </summary>
        /// 
        /// <returns>
        /// The message that was created.
        /// </returns>
        /// <param name="requestUri">The URI from which to create the message. Message parameters are specified in the query string. The wa parameter must be present.</param><exception cref="T:System.ArgumentNullException"><paramref name="requestUri"/> is null.</exception><exception cref="T:System.IdentityModel.Services.WSFederationMessageException">A message cannot be created from the specified URI.</exception>
        public static WSFederationMessage CreateFromUri(Uri requestUri)
        {
            if (requestUri == null)
            {
                throw new ArgumentNullException(nameof(requestUri));
            }

            WSFederationMessage fedMsg;
            if (TryCreateFromUri(requestUri, out fedMsg))
            {
                return fedMsg;
            }

            throw new InvalidOperationException(string.Format("ID3094: Cannot create WS-Federation message from the given URI '{0}'", requestUri));
        }

        /// <summary>
        /// Attempts to create a WS-Federation message from the specified URI. The parameters are assumed to be specified as a query string.
        /// </summary>
        /// 
        /// <returns>
        /// true if a message was successfully created; otherwise, false.
        /// </returns>
        /// <param name="requestUri">The URI from which to create the message. Message parameters are specified in the query string. The wa parameter must be present.</param><param name="fedMsg">When this method returns, contains the message that was created or null if a message could not be created. This parameter is treated as uninitialized.</param><exception cref="T:System.ArgumentNullException"><paramref name="requestUri"/> is null.</exception>
        public static bool TryCreateFromUri(Uri requestUri, out WSFederationMessage fedMsg)
        {
            if (requestUri == null)
            {
                throw new ArgumentNullException(nameof(requestUri));
            }

            var baseUrl = GetBaseUrl(requestUri);
            fedMsg = CreateFromNameValueCollection(baseUrl, ParseQueryString(requestUri));
            return fedMsg != null;
        }

        /// <summary>
        /// Creates a WS-Federation message from a <see cref="T:System.Collections.Specialized.NameValueCollection"/> of parameters.
        /// </summary>
        /// 
        /// <returns>
        /// The message that was created or null if a message cannot be created.
        /// </returns>
        /// <param name="baseUrl">The base URL to which the message is intended.</param><param name="collection">The <see cref="T:System.Collections.Specialized.NameValueCollection"/> that contains the parameters for the message.</param><exception cref="T:System.ArgumentNullException"><paramref name="baseUrl"/> is null. -or-<paramref name="collection"/> is null.</exception><exception cref="T:System.IdentityModel.Services.WSFederationMessageException">The wa parameter in the parameter collection is not recognized.</exception><exception cref="T:System.ArgumentException">A sign-in response message has both the wresult and wresultptr parameter in the parameter collection. (A valid sign-in response message has the wa parameter equal to “wsignin1.0” and either the wresult or the wresultptr parameter, but not both.)</exception>
        public static WSFederationMessage CreateFromNameValueCollection(Uri baseUrl, Dictionary<string, StringValues> collection)
        {
            if (baseUrl == null)
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            WSFederationMessage message = null;
            var str = collection.Get("wa");
            if (!string.IsNullOrEmpty(str))
            {
                if (!(str == "wattr1.0"))
                {
                    if (!(str == "wpseudo1.0"))
                    {
                        if (!(str == "wsignin1.0"))
                        {
                            if (!(str == "wsignout1.0"))
                            {
                                if (str == "wsignoutcleanup1.0")
                                {
                                    message = new SignOutCleanupRequestMessage(baseUrl);
                                }
                                else
                                {
                                    throw new InvalidOperationException(string.Format("ID3014: Unrecognized Action name: '{0}'", str));
                                }
                            }
                            else
                            {
                                message = new SignOutRequestMessage(baseUrl);
                            }
                        }
                        else
                        {
                            string result = collection.Get("wresult");
                            string uriString = collection.Get("wresultptr");
                            int num = !string.IsNullOrEmpty(result) ? 1 : 0;
                            bool flag = !string.IsNullOrEmpty(uriString);
                            if (num != 0)
                            {
                                message = new SignInResponseMessage(baseUrl, result);
                            }
                            else if (flag)
                            {
                                message = new SignInResponseMessage(baseUrl, new Uri(uriString));
                            }
                            else
                            {
                                string realm = collection.Get("wtrealm");
                                string reply = collection.Get("wreply");
                                message = new SignInRequestMessage(baseUrl, realm, reply);
                            }
                        }
                    }
                    else
                        message = new PseudonymRequestMessage(baseUrl);
                }
                else
                    message = new AttributeRequestMessage(baseUrl);
            }
            if (message != null)
            {
                WSFederationMessage.PopulateMessage(message, collection);
                message.Validate();
            }
            return message;
        }

        /// <summary>
        /// Creates a WS-Federation message from the form post received in the specified request.
        /// </summary>
        /// 
        /// <returns>
        /// The message that was created or null if a message cannot be created.
        /// </returns>
        /// <param name="request">The request that contains the form post.</param><exception cref="T:System.ArgumentNullException"><paramref name="request"/> is null.</exception>
        /*
        public static WSFederationMessage CreateFromFormPost(HttpRequestBase request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return WSFederationMessage.CreateFromNameValueCollection(FederationMessage.GetBaseUrl(request.Url), request.Unvalidated.Form);
        }
        */

        private static void PopulateMessage(WSFederationMessage message, Dictionary<string, StringValues> collection)
        {
            foreach (string parameter in collection.Keys)
            {
                if (string.IsNullOrEmpty(collection[parameter]))
                {
                    if (parameter == "wtrealm" || parameter == "wresult")
                    {
                        throw new InvalidOperationException(string.Format("ID3261: The WS-Federation parameter '{0}' is null or empty.", parameter));
                    }
                }
                else
                {
                    message.SetParameter(parameter, collection[parameter]);
                }
            }
        }

        #endregion
    }
}
