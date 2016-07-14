using System;
using System.IO;

namespace WsFederation.Messages
{
    public class SignInResponseMessage : WSFederationMessage
    {
        #region Properties

        /// <summary>
        /// Gets or sets the wresult parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// The value of the wresult parameter.
        /// </returns>
        public string Result
        {
            get
            {
                return GetParameter("wresult");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    RemoveParameter("wresult");
                }
                else
                {
                    SetParameter("wresult", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the wresultptr parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// The value of the wresultptr parameter.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">An attempt to set a value that is not a valid, absolute URI occurs. Can be null or empty.</exception>
        public string ResultPtr
        {
            get
            {
                return GetParameter("wresultptr");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    RemoveParameter("wresultptr");
                }
                else
                {
                    SetUriParameter("wresultptr", value);
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IdentityModel.Services.SignInResponseMessage"/> class with the specified base URL and wresult parameter.
        /// </summary>
        /// <param name="baseUrl">The base URL to which the Sign-In Response message applies.</param><param name="result">The wresult parameter in the message.</param><exception cref="T:System.ArgumentException"><paramref name="result"/> is null or empty.</exception>
        public SignInResponseMessage(Uri baseUrl, string result)
          : base(baseUrl, "wsignin1.0")
        {
            if (string.IsNullOrEmpty(result))
            {
                throw new ArgumentNullException(nameof(result));
            }

            SetParameter("wresult", result);
        }

        /// <summary>
        /// Initializes an instance of the <see cref="T:System.IdentityModel.Services.SignInResponseMessage"/> class using the specified base URL and wresultptr parameter.
        /// </summary>
        /// <param name="baseUrl">The base URL to which the Sign-In Response message applies.</param><param name="resultPtr">The wresultptr parameter in the message.</param><exception cref="T:System.ArgumentNullException"><paramref name="resultPtr"/> is null.</exception>
        public SignInResponseMessage(Uri baseUrl, Uri resultPtr)
          : base(baseUrl, "wsignin1.0")
        {
            if (resultPtr == null)
            {
                throw new ArgumentNullException(nameof(resultPtr));
            }

            SetParameter("wresultptr", resultPtr.AbsoluteUri);
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Validates the current instance.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">The action parameter (wa) is not “wsignin1.0”.</exception><exception cref="T:System.IdentityModel.Services.WSFederationMessageException">Neither the wresult parameter nor the wresultptr parameter is specified-or-Both the wresult parameter and the wresultptr parameter are specified.</exception>
        protected override void Validate()
        {
            base.Validate();
            string parameter = this.GetParameter("wa");
            if (parameter != "wsignin1.0")
            {
                throw new InvalidOperationException(string.Format("ID3000: Federation message has an unrecognized Action '{0}'", parameter));
            }

            var flag1 = !string.IsNullOrEmpty(GetParameter("wresult"));
            var flag2 = !string.IsNullOrEmpty(GetParameter("wresultptr"));
            if (flag1 & flag2)
            {
                throw new InvalidOperationException("ID3016: SignInResponseMessage cannot have wresult and wresultptr parameters set at the same time.");
            }
            if (!(flag1 | flag2))
            {
                throw new InvalidOperationException("ID3001: Either wresult or wresultptr parameter needs to be specified for a SignInResponseMessage.");
            }
        }

        /// <summary>
        /// Writes this message in a form post format to the specified text writer.
        /// </summary>
        /// <param name="writer">The text writer to which to write the message.</param><exception cref="T:System.ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public override void Write(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            Validate();
            writer.Write(this.WriteFormPost());
        }

        #endregion
    }
}
