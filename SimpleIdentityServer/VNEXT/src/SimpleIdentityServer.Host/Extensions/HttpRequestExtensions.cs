using System;
using System.Net.Http;
using Microsoft.AspNet.Http;

namespace SimpleIdentityServer.Host.Extensions
{
    public static class HttpRequestsExtensions
    {
        public static string GetAbsoluteUriWithVirtualPath(this HttpRequest requestMessage)
        {
            return requestMessage.PathBase;
        }
    }
}