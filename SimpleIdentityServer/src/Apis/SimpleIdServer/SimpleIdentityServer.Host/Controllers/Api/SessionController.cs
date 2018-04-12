using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.Controllers.Api
{
    public class SessionController : Controller
    {
        [HttpGet(Constants.EndPoints.CheckSession)]
        public async Task CheckSession()
        {
            // 1. GET THE USER.
            // 2. GET THE SESSION ID & INSERT IT INTO THE HTML.
            // 3. RETURNS THE SESSION ID.
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "SimpleIdentityServer.Host.Views.CheckSession.html";
            string html;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }
            }
            
            Response.ContentType = "text/html; charset=UTF-8";
            var payload = Encoding.UTF8.GetBytes(html);
            await Response.Body.WriteAsync(payload, 0, payload.Length);
        }

        [HttpHead(Constants.EndPoints.EndSession)]
        public async Task RevokeSession()
        {
            // 1. GET THE USER.
            // 2. RETRIEVES THE PARAMETER (id_token_type + post_logout_red + state).
            // 3. DISPLAY VIEW (are-you sure to logout ?)
        }
    }
}
