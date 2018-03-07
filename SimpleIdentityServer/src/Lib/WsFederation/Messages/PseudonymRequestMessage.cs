using System;
using System.IO;

namespace WsFederation.Messages
{
    public class PseudonymRequestMessage : WSFederationMessage
    {
        #region Properties

        /// <summary>
        /// Gets or sets the wpseudo parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// A string that contains the value of the wpseudo parameter.
        /// </returns>
        public string Pseudonym
        {
            get
            {
                return GetParameter("wpseudo");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    RemoveParameter("wpseudo");
                }
                else
                {
                    SetParameter("wpseudo", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the wpseudoptr parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// A string that contains the value of the wpseudoptr parameter.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">An attempt to set a value that is not a valid URI occurs.</exception>
        public string PseudonymPtr
        {
            get
            {
                return GetParameter("wpseudoptr");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    RemoveParameter("wpseudoptr");
                }
                else
                {
                    SetUriParameter("wpseudoptr", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Reply parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// A string that contains the value of the wreply parameter. This is the URL to which the reply should be sent.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">An attempt to set a value that is not a valid URI occurs.</exception>
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
        /// Gets or sets the wresult parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// A string that contains the value of the wresult parameter.
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
        /// A string that contains the value of the wresultptr parameter. This is a URI.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">An attempt to set a value that is not a valid URI occurs.</exception>
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
        /// Initializes a new instance of the <see cref="T:System.IdentityModel.Services.PseudonymRequestMessage"/> class with the specified base URL.
        /// </summary>
        /// <param name="baseUrl">The base URL to which this message applies.</param>
        public PseudonymRequestMessage(Uri baseUrl)
          : base(baseUrl, "wpseudo1.0")
        {
        }

        #endregion

        #region Public methods
        
        /// <summary>
        /// Writes this message in a query string form to the specified text writer.
        /// </summary>
        /// <param name="writer">The text writer to which to write the message.</param><exception cref="T:System.ArgumentNullException"><paramref name="writer"/> is null.</exception>
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
        /// No validation is performed by the framework. Users of this class should validate externally.
        /// </summary>
        protected override void Validate()
        {
        }

        #endregion
    }
}
