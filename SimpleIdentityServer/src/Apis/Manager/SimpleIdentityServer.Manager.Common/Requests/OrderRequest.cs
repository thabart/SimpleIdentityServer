using System.Runtime.Serialization;

namespace SimpleIdentityServer.Manager.Common.Requests
{
    [DataContract]
    public class OrderRequest
    {
        [DataMember(Name = Constants.OrderRequestNames.Target)]
        public string Target { get; set; }

        [DataMember(Name = Constants.OrderRequestNames.Type)]
        public int Type { get; set; }
    }
}
