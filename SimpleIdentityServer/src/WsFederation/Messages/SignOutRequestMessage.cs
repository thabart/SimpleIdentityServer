using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WsFederation.Messages
{
    public class SignOutRequestMessage : WSFederationMessage
    {
        #region Properties

        /// <summary>
        /// Gets or sets the wreply parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// The value of the wreply parameter. This is the URL to which the browser should be redirected.
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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IdentityModel.Services.SignOutRequestMessage"/> class with the specified base URL.
        /// </summary>
        /// <param name="baseUrl">The base URL to which this message applies. Sets the  <see cref=""/> property.</param><exception cref="T:System.ArgumentNullException"><paramref name="baseUrl"/> is null.</exception><exception cref="T:System.IdentityModel.Services.WSFederationMessageException"><paramref name="baseUrl"/> is not a valid, absolute URI.</exception>
        public SignOutRequestMessage(Uri baseUrl)
          : base(baseUrl, "wsignout1.0")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IdentityModel.Services.SignOutRequestMessage"/> class with the specified base URL and wreply parameter.
        /// </summary>
        /// <param name="baseUrl">The base URL to which this message applies.</param><param name="reply">The value of the wreply parameter. The URL to which the reply should be sent.</param><exception cref="T:System.ArgumentException"><paramref name="reply"/> is either empty or null.-or-<paramref name="reply"/> is not a valid, absolute URI.</exception><exception cref="T:System.ArgumentNullException"><paramref name="baseUrl"/> is null.</exception><exception cref="T:System.IdentityModel.Services.WSFederationMessageException"><paramref name="baseUrl"/> is not a valid, absolute URI.</exception>
        public SignOutRequestMessage(Uri baseUrl, string reply)
          : base(baseUrl, "wsignout1.0")
        {
            this.SetUriParameter("wreply", reply);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Writes the message in query string form to the specified text writer.
        /// </summary>
        /// <param name="writer">The writer to which to write the message.</param><exception cref="T:System.ArgumentNullException"><paramref name="writer"/> is null.</exception>
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
        /// <exception cref="T:System.InvalidOperationException">The wa parameter (the <see cref="P:System.IdentityModel.Services.WSFederationMessage.Action"/> property) is not set to “wsignout1.0”.</exception>
        protected override void Validate()
        {
            base.Validate();
            string parameter = GetParameter("wa");
            if (string.IsNullOrEmpty(parameter) || !parameter.Equals("wsignout1.0"))
            {
                throw new InvalidOperationException(string.Format("ID3000: Federation message has an unrecognized Action '{0}'.", parameter));
            }
        }

        #endregion
    }
}
