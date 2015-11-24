using System;
using System.Net.Http;

namespace SimpleIdentityServer.Api.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static string GetAbsoluteUriWithVirtualPath(this HttpRequestMessage requestMessage)
        {
            var leftPart = requestMessage.RequestUri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
            var virtualPath = requestMessage.GetRequestContext().VirtualPathRoot.TrimEnd('/');
            return leftPart + virtualPath;
        }
    }
}