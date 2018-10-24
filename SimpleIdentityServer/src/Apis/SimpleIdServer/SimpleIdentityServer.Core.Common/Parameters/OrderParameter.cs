namespace SimpleIdentityServer.Core.Common.Parameters
{
    public enum OrderTypes
    {
        Asc,
        Desc
    }

    public class OrderParameter
    {
        public string Target { get; set; }
        public OrderTypes Type { get; set; }
    }
}
