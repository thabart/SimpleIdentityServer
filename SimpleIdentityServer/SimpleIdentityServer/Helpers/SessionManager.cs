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
        public static string CookieName = "SimpleIdentityServerName";

        private IProtector _protector;

        public SessionManager(IProtector protector)
        {
            _protector = protector;
        }

        public string GetSession()
        {
            var cookie = HttpContext.Current.Request.Cookies[CookieName];
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
            var httpCookie = new HttpCookie(CookieName, encryptedSubject);
            HttpContext.Current.Response.SetCookie(httpCookie);
        }
    }
}