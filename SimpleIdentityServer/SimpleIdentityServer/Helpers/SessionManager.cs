using SimpleIdentityServer.Core.Protector;
using System.Web;

namespace SimpleIdentityServer.Api.Helpers
{
    public interface ISessionManager
    {
        void StoreSession(string subject);

        string GetSession();
    }

    public class SessionManager : ISessionManager
    {
        private static string _cookieName = "SimpleIdentityServerName";

        private IProtector _protector;

        public SessionManager(IProtector protector)
        {
            _protector = protector;
        }

        public string GetSession()
        {
            var cookie = HttpContext.Current.Request.Cookies[_cookieName];
            if (cookie == null || string.IsNullOrWhiteSpace(cookie.Value))
            {
                return string.Empty;
            }

            var encryptedValue = cookie.Value;
            return _protector.Decrypt<string>(encryptedValue);
        }


        public void StoreSession(string subject)
        {
            var encryptedSubject = _protector.Encrypt(subject);
            var httpCookie = new HttpCookie(_cookieName, encryptedSubject);
            HttpContext.Current.Response.SetCookie(httpCookie);
        }
    }
}