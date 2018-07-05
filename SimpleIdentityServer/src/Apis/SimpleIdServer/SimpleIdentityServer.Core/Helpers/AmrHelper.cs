using SimpleIdentityServer.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Core.Helpers
{
    public interface IAmrHelper
    {
        string GetAmr(IEnumerable<string> currentAmrs, IEnumerable<string> exceptedAmrs);
    }

    internal sealed class AmrHelper : IAmrHelper
    {
        public AmrHelper()
        {
        }

        public string GetAmr(IEnumerable<string> currentAmrs, IEnumerable<string> exceptedAmrs)
        {
            if (currentAmrs == null || !currentAmrs.Any())
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.NoActiveAmr);
            }

            var amr = Constants.DEFAULT_AMR;
            if (exceptedAmrs != null)
            {
                foreach(var exceptedAmr in exceptedAmrs)
                {
                    if (currentAmrs.Contains(exceptedAmr))
                    {
                        amr = exceptedAmr;
                        break;
                    }
                }
            }

            if (!currentAmrs.Contains(amr))
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, string.Format(Errors.ErrorDescriptions.TheAmrDoesntExist, amr));
            }

            return amr;
        }
    }
}
