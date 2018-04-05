using System;

namespace SimpleIdentityServer.ResourceManager.Core.Models
{
    public enum EndpointTypes
    {
        AUTH,
        OPENID,
        SCIM
    }

    public class EndpointAggregate
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public EndpointTypes Type { get; set; }
        public DateTime CreateDateTime { get; set; }
        public int Order { get; set; }
        public string ManagerUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthUrl { get; set; }
    }
}
