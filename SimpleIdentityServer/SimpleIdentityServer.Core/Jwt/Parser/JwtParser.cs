using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Jwt.Parser
{
    public interface IJwtParser
    {

    }

    public class JwtParser
    {
        public JwsPayload UnSigned(string signedParameter)
        {
            return null;
        }
    }
}
