using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace WsFederation.Messages
{
    public class SignInRequestMessage : WSFederationMessage
    {
        #region Properties

        /// <summary>
        /// Gets a string representation of the URL that corresponds to this message.
        /// </summary>
        /// 
        /// <returns>
        /// A URL serialized from the current instance.
        /// </returns>
        public string RequestUrl
        {
            get
            {
                var sb = new StringBuilder(128);
                using (var stringWriter = new StringWriter(sb, CultureInfo.InvariantCulture))
                {
                    Write(stringWriter);
                    return sb.ToString();
                }
            }
        }

        /// <summary>
        /// Gets or sets the wfed parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// The value of the wfed parameter. This is specified as a URI.
        /// </returns>
        public string Federation
        {
            get
            {
                return GetParameter("wfed");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    RemoveParameter("wfed");
                }
                else
                {
                    SetParameter("wfed", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the wreply parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// The value of the wreply parameter. This is specified as a URI.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">An attempt to set a value that is not a valid, absolute URI occurs.</exception>
        public string Reply
        {
            get
            {
                return GetParameter("wreply");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    RemoveParameter("wreply");
                }
                else
                {
                    SetUriParameter("wreply", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the wct parameter of the message.
        /// 
        /// </summary>
        /// 
        /// <returns>
        /// The value of the wct parameter specified as a datetime string in UTC.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">An attempt to set a value that is not a valid datetime string occurs.</exception>
        public string CurrentTime
        {
            get
            {
                return GetParameter("wct");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    RemoveParameter("wct");
                }
                else
                {
                    SetParameter("wct", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the wfresh parameter of the message.
        /// 
        /// </summary>
        /// 
        /// <returns>
        /// The value of the wfresh parameter. This should be an integer represented as a string. It specifies the maximum age in minutes that the authentication is valid. Zero indicates that the user should be prompted before the token is issued.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">An attempt to set a value that is not a string representation of an integer.</exception>
        public string Freshness
        {
            get
            {
                return GetParameter("wfresh");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    RemoveParameter("wfresh");
                }
                else
                {
                    SetParameter("wfresh", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the whr parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// The value of the whr parameter. This is specified as a URI.
        /// </returns>
        public string HomeRealm
        {
            get
            {
                return this.GetParameter("whr");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.RemoveParameter("whr");
                }
                else
                {
                    this.SetParameter("whr", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the wauth parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// The authentication type. This is specified as a URI.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">An attempt to set a value that is not a valid, absolute URI occurs.</exception>
        public string AuthenticationType
        {
            get
            {
                return GetParameter("wauth");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    RemoveParameter("wauth");
                }
                else
                {
                    SetUriParameter("wauth", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the wp parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// The value of the wp parameter. This is specified as a URI.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">An attempt to set a value that is not a valid, absolute URI occurs.</exception>
        public string Policy
        {
            get
            {
                return GetParameter("wp");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    RemoveParameter("wp");
                }
                else
                {
                    SetUriParameter("wp", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the wres parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// The value of the wres parameter.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">An attempt to set a value that is not a valid, absolute URI occurs.</exception>
        public string Resource
        {
            get
            {
                return GetParameter("wres");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    RemoveParameter("wres");
                }
                else
                {
                    SetUriParameter("wres", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the wtrealm parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// The value of the wtrealm parameter. This is specified as a URI.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">An attempt to set a value that is not a valid, absolute URI occurs.</exception>
        public string Realm
        {
            get
            {
                return this.GetParameter("wtrealm");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    this.RemoveParameter("wtrealm");
                else
                    this.SetUriParameter("wtrealm", value);
            }
        }

        /// <summary>
        /// Gets or sets the wreq parameter of the message.
        /// 
        /// </summary>
        /// 
        /// <returns>
        /// The value of the wreq parameter.
        /// </returns>
        public string Request
        {
            get
            {
                return this.GetParameter("wreq");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    this.RemoveParameter("wreq");
                else
                    this.SetParameter("wreq", value);
            }
        }

        /// <summary>
        /// Gets or sets the wreqptr parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// The value of the wreqptr parameter.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">An attempt to set a value that is not a valid, absolute URI occurs.</exception>
        public string RequestPtr
        {
            get
            {
                return GetParameter("wreqptr");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    RemoveParameter("wreqptr");
                }
                else
                {
                    SetUriParameter("wreqptr", value);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IdentityModel.Services.SignInRequestMessage"/> class with the specified base URL and wtrealm parameter.
        /// </summary>
        /// <param name="baseUrl">The base URL to which the sign-in message applies.</param><param name="realm">The value of the wtrealm message parameter. Sets the <see cref="P:System.IdentityModel.Services.SignInRequestMessage.Realm"/> property.</param><exception cref="T:System.ArgumentNullException"><paramref name="realm"/> is null or an empty string.</exception>
        public SignInRequestMessage(Uri baseUrl, string realm)
          : this(baseUrl, realm, (string)null)
        {
        }

        internal SignInRequestMessage(Uri baseUrl)
          : base(baseUrl, "wsignin1.0")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IdentityModel.Services.SignInRequestMessage"/> class using the specified base URI, wtrealm parameter, and wreply parameter. Supports non-standard message creation for backward compatibility.
        /// </summary>
        /// <param name="baseUrl">The Base URL to which the sign-in message applies.</param><param name="realm">The value of the wtrealm message parameter. If not null or empty, sets the <see cref="P:System.IdentityModel.Services.SignInRequestMessage.Realm"/> property. </param><param name="reply">The URI to which to reply. (The value of the wreply message parameter.) If not null or empty, sets the <see cref="P:System.IdentityModel.Services.SignInRequestMessage.Reply"/> property.</param><exception cref="T:System.ArgumentNullException">Both <paramref name="realm"/> and <paramref name="reply"/> are null or an empty string.</exception>
        public SignInRequestMessage(Uri baseUrl, string realm, string reply)
          : base(baseUrl, "wsignin1.0")
        {
            if (string.IsNullOrEmpty(realm) && string.IsNullOrEmpty(reply))
            {
                throw new ArgumentNullException(nameof(realm));
            }

            if (!string.IsNullOrEmpty(realm))
            {
                SetParameter("wtrealm", realm);
            }

            if (string.IsNullOrEmpty(reply))
            {
                return;
            }

            SetParameter("wreply", reply);
        }

        #endregion

        #region Public methods
                
        /// <summary>
        /// Writes this message in query string form to the specified text writer.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.IO.TextWriter"/> to which to write the message.</param><exception cref="T:System.ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void Write(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            Validate();
            writer.Write(this.WriteQueryString());
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Validates the current instance.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">The wa parameter (the <see cref="P:System.IdentityModel.Services.WSFederationMessage.Action"/> property) is not set to “wsignin1.0”. </exception><exception cref="T:System.IdentityModel.Services.WSFederationMessageException">Neither the wtrealm parameter nor the wreply parameter is present. (The <see cref="P:System.IdentityModel.Services.SignInRequestMessage.Realm"/> property and the <see cref="P:System.IdentityModel.Services.SignInRequestMessage.Reply"/> property are null or empty.)-or-Both the wreq parameter and the wreqptr parameter are present. (The <see cref="P:System.IdentityModel.Services.SignInRequestMessage.Request"/> property and the <see cref="P:System.IdentityModel.Services.SignInRequestMessage.RequestPtr"/> property are both set.)</exception>
        protected override void Validate()
        {
            base.Validate();
            string parameter = this.GetParameter("wa");
            if (parameter != "wsignin1.0")
            {
                throw new InvalidOperationException(string.Format("ID3000: Federation message has an unrecognized Action '{0}'.", parameter));
            }

            if (string.IsNullOrEmpty(this.GetParameter("wtrealm")) && string.IsNullOrEmpty(GetParameter("wreply")))
            {
                throw new InvalidOperationException("ID3204: WS - Federation SignIn request must specify a 'wtrealm' or 'wreply' parameter.");
            }

            if (!string.IsNullOrEmpty(this.GetParameter("wreq")) && !string.IsNullOrEmpty(GetParameter("wreqptr")))
            {
                throw new InvalidOperationException("ID3142: WS-Federation SignInRequestMessage cannot have the 'wreq' and 'wreqptr' parameter set at the same time.");
            }
        }

        #endregion
    }
}
