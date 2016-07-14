using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace WsFederation.Messages
{
    public abstract class FederationMessage
    {
        #region Fields

        private Dictionary<string, string> _parameters = new Dictionary<string, string>();
        private Uri _baseUri;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the message parameters as a dictionary.
        /// </summary>
        /// 
        /// <returns>
        /// A dictionary that contains the message parameters.
        /// </returns>
        public IDictionary<string, string> Parameters
        {
            get
            {
                return _parameters;
            }
        }

        /// <summary>
        /// Gets or sets the base URL to which the message applies.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.Uri"/> that contains the base URL.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">An attempt to set a value that is null occurs.</exception><exception cref="T:System.IdentityModel.Services.WSFederationMessageException">An attempt to set a value that is not a valid URI occurs.</exception>
        public Uri BaseUri
        {
            get
            {
                return this._baseUri;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }


                _baseUri = value;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// When overridden in a derived class, writes the message to the output stream.
        /// </summary>
        /// <param name="writer">The text writer to which the message is written out.</param>
        public abstract void Write(TextWriter writer);

        /// <summary>
        /// Returns the specified parameter value from the parameters dictionary.
        /// </summary>
        /// 
        /// <returns>
        /// The value of the parameter or null if the parameter does not exist.
        /// </returns>
        /// <param name="parameter">The parameter for which to search.</param><exception cref="T:System.ArgumentException"><paramref name="parameter"/> is null or an empty string.</exception>
        public string GetParameter(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var str = string.Empty;
            _parameters.TryGetValue(parameter, out str);
            return str;
        }

        /// <summary>
        /// Sets the value of a parameter in the parameters dictionary.
        /// </summary>
        /// <param name="parameter">The name of the parameter to set.</param><param name="value">The value to be assigned to the parameter.</param><exception cref="T:System.ArgumentException"><paramref name="parameter"/> is null or an empty string. -or-<paramref name="value"/> is null or an empty string.</exception>
        public void SetParameter(string parameter, string value)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            this._parameters[parameter] = value;
        }

        /// <summary>
        /// Sets the value of a parameter in the parameters dictionary. The value must be an absolute URI.
        /// </summary>
        /// <param name="parameter">The parameter name.</param><param name="value">The parameter value.</param><exception cref="T:System.ArgumentException"><paramref name="parameter"/> is null.-or-<paramref name="value"/> is null or not an absolute URI.</exception>
        public void SetUriParameter(string parameter, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            SetParameter(parameter, value);
        }

        /// <summary>
        /// Removes a parameter from the parameters dictionary.
        /// </summary>
        /// <param name="parameter">The name of the parameter to remove.</param><exception cref="T:System.ArgumentException"><paramref name="parameter"/> is null or an empty string.</exception>
        public void RemoveParameter(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (!Parameters.Keys.Contains(parameter))
            {
                return;
            }

            _parameters.Remove(parameter);
        }
        
        /// <summary>
        /// Returns a string representation of the message in query-string format.
        /// </summary>
        /// 
        /// <returns>
        /// The message in query-string format.
        /// </returns>
        public virtual string WriteQueryString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(this._baseUri.AbsoluteUri);
            stringBuilder.Append("?");
            bool flag = true;
            foreach (KeyValuePair<string, string> keyValuePair in this._parameters)
            {
                if (!flag)
                {
                    stringBuilder.Append("&");
                }

                stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0}={1}", new []
                {
                  WebUtility.UrlEncode(keyValuePair.Key),
                  WebUtility.UrlEncode(keyValuePair.Value)
                }));
                flag = false;
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Serializes the message as a form post and returns the resulting Form together with its Javascript as a string.
        /// </summary>
        /// 
        /// <returns>
        /// A string representation of the message as a Form together with its associated Javascript.
        /// </returns>
        public virtual string WriteFormPost()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "<html><head><title>{0}</title></head><body><form method=\"POST\" name=\"hiddenform\" action=\"{1}\">", new []
            {
                "Working...",
                WebUtility.HtmlEncode(_baseUri.AbsoluteUri)
            }));
            foreach (var keyValuePair in _parameters)
            {
                stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />", new []
                {
                    WebUtility.HtmlEncode(keyValuePair.Key),
                    WebUtility.HtmlEncode(keyValuePair.Value)
                }));
            }

            stringBuilder.Append("<noscript><p>");
            stringBuilder.Append("Script is disabled. Click Submit to continue.");
            stringBuilder.Append("</p><input type=\"submit\" value=\"");
            stringBuilder.Append("Submit");
            stringBuilder.Append("\" /></noscript>");
            stringBuilder.Append("</form><script language=\"javascript\">window.setTimeout('document.forms[0].submit()', 0);</script></body></html>");
            return stringBuilder.ToString();
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Called from constructors in derived classes to initialize the <see cref="T:System.IdentityModel.Services.FederationMessage"/> class.
        /// </summary>
        /// <param name="baseUrl">The base URL to which the federation message applies. Initializes the <see cref="P:System.IdentityModel.Services.FederationMessage.BaseUri"/> property.</param><exception cref="T:System.ArgumentNullException"><paramref name="baseUri"/> is null.</exception><exception cref="T:System.IdentityModel.Services.WSFederationMessageException"><paramref name="baseUri"/> is not a valid, absolute URI.</exception>
        protected FederationMessage(Uri baseUrl)
        {
            if (baseUrl == null)
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            _baseUri = baseUrl;
        }

        /// <summary>
        /// Validates the message.
        /// </summary>
        /// <exception cref="T:System.IdentityModel.Services.WSFederationMessageException">The value of the <see cref="P:System.IdentityModel.Services.FederationMessage.BaseUri"/> property is null or is not an absolute URI.</exception>
        protected virtual void Validate()
        {
            if (_baseUri == null)
            {
                throw new ArgumentNullException(nameof(_baseUri));
            }
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Helper method that parses the query string in the specified URI into a <see cref="T:System.Collections.Specialized.NameValueCollection"/>.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.Collections.Specialized.NameValueCollection"/> that contains the parameters in the query string.
        /// </returns>
        /// <param name="data">The URI to parse.</param><exception cref="T:System.ArgumentNullException"><paramref name="data"/> is null.</exception>
        public static Dictionary<string, StringValues> ParseQueryString(Uri data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return QueryHelpers.ParseQuery(data.Query);
        }

        

        /// <summary>
        /// Helper method that extracts the base URL from the specified URI.
        /// </summary>
        /// 
        /// <returns>
        /// The base URL that was extracted.
        /// </returns>
        /// <param name="uri">The URI from which to extract the base URL.</param>
        public static Uri GetBaseUrl(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            var uriString = uri.AbsoluteUri;
            var length = uriString.IndexOf("?", 0, StringComparison.Ordinal);
            if (length > -1)
            {
                uriString = uriString.Substring(0, length);
            }

            return new Uri(uriString);
        }

        #endregion
    }
}
