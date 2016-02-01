using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;

namespace SimpleIdentityServer.Host.Extensions
{
    public static class HttpRequestsExtensions
    {
        public static string GetAbsoluteUriWithVirtualPath(this HttpRequest requestMessage)
        {
            return requestMessage.PathBase;
        }
        
        public static async Task<string> ReadAsStringAsync(this HttpRequest request) {
            request.Body.Position = 0;
            using (var reader = new StreamReader(request.Body)) 
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}