using System;

namespace SimpleIdentityServer.RateLimitation.Models
{
    public class CacheableResponse
    {
        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public string IpAdress { get; set; }

        public DateTime UpdateDateTime { get; set; }

        public int NumberOfRequests { get; set; }

        public string GetIdentifier()
        {
            return string.Format("{0} {1} {2}",
                ControllerName,
                ActionName,
                IpAdress);
        }

        public override bool Equals(object obj)
        {
            var second = obj as CacheableResponse;
            if (second == null)
            {
                return false;
            }

            return second.ControllerName == ControllerName &&
                   second.ActionName == ActionName &&
                   second.IpAdress == IpAdress;
        }

        public override int GetHashCode()
        {
            return ControllerName.GetHashCode() ^
                   ActionName.GetHashCode() ^
                   IpAdress.GetHashCode();
        }
    }
}