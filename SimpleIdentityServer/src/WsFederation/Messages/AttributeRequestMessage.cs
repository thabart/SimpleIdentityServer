using System;
using System.IO;

namespace WsFederation.Messages
{
    public class AttributeRequestMessage : WSFederationMessage
    {
        #region Properties

        /// <summary>
        /// Gets or sets the wattr parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// A string that contains the value of the wattr parameter.
        /// </returns>
        public string Attribute
        {
            get
            {
                return GetParameter("wattr");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    RemoveParameter("wattr");
                }
                else
                {
                    SetParameter("wattr", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the wattrptr parameter of the message.
        /// </summary>
        /// 
        /// <returns>
        /// A string that contains the value of the wattrptr parameter.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">An attempt to set a value that is not a valid URI occurs.</exception>
        public string AttributePtr
        {
            get
            {
                return GetParameter("wattrptr");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    RemoveParameter("wattrptr");
                }
                else
                {
                    SetUriParameter("wattrptr", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the wreply parameter of the message.
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
        /// A string that contains the value of the wresultptr parameter.
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
        /// Initializes a new instance of the <see cref="T:System.IdentityModel.Services.AttributeRequestMessage"/> class with the specified base URL.
        /// </summary>
        /// <param name="baseUrl">The base URL to which this message applies.</param>
        public AttributeRequestMessage(Uri baseUrl)
          : base(baseUrl, "wattr1.0")
        {
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
        /// No validation is performed by the framework. Users of this class should validate externally.
        /// </summary>
        protected override void Validate()
        {
        }

        #endregion
    }
}
