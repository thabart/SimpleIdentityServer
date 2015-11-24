using System;
using System.Net.Http;

namespace SimpleIdentityServer.Api.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static Uri GetAbsoluteUriWithVirtualPath(this HttpRequestMessage requestMessage)
        {
            var leftPart = requestMessage.RequestUri.GetLeftPart(UriPartial.Authority);
            var virtualPath = requestMessage.GetRequestContext().VirtualPathRoot.TrimEnd('/');
            return new Uri(new Uri(leftPart), virtualPath);
        }
    }
}