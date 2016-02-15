namespace SimpleIdentityServer.DataAccess.Fake.Models
{
    public class Translation
    {
        public string Code { get; set; }

        /// <summary>
        /// Naming convention of the language tag is defined by the RFC : http://tools.ietf.org/html/rfc5646
        /// </summary>
        public string LanguageTag { get; set; }

        public string Value { get; set; }
    }
}
