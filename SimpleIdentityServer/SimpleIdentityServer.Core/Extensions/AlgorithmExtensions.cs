using System;

using SimpleIdentityServer.Core.Jwt.Signature;

namespace SimpleIdentityServer.Core.Extensions
{
    public static class AlgorithmExtensions
    {
        public static AllAlg ToAllAlg(this JwsAlg alg)
        {
            var name = Enum.GetName(typeof (JwsAlg), alg);
            return (AllAlg)Enum.Parse(typeof (AllAlg), name);
        }

        public static AllAlg ToAllAlg(this JweAlg alg)
        {
            var name = Enum.GetName(typeof(JweAlg), alg);
            return (AllAlg)Enum.Parse(typeof(AllAlg), name);
        }
    }
}
