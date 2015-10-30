using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Api.Parsers
{
    public interface IRedirectInstructionParser
    {
        ActionInformation GetActionInformation(RedirectInstruction action);

        RouteValueDictionary GetRouteValueDictionary(RedirectInstruction instruction);
    }

    public class RedirectInstructionParser : IRedirectInstructionParser
    {
        private readonly Dictionary<IdentityServerEndPoints, ActionInformation> _mappingEnumToActionInformations = new Dictionary<IdentityServerEndPoints, ActionInformation>
        {
            {
                IdentityServerEndPoints.ConsentIndex,
                new ActionInformation("Consent", "Index")
            }, 
            {
                IdentityServerEndPoints.AuthenticateIndex,
                new ActionInformation("Authenticate", "Index")
            }
        };

        public ActionInformation GetActionInformation(RedirectInstruction instruction)
        {
            if (!_mappingEnumToActionInformations.ContainsKey(instruction.Action))
            {
                return null;
            }

            var actionInformation = _mappingEnumToActionInformations[instruction.Action];
            var dic = GetRouteValueDictionary(instruction);
            actionInformation.RouteValueDictionary = dic;
            return actionInformation;
        }

        public RouteValueDictionary GetRouteValueDictionary(RedirectInstruction instruction)
        {
            var result = new RouteValueDictionary();
            if (instruction.Parameters != null && instruction.Parameters.Any())
            {
                foreach (var parameter in instruction.Parameters)
                {
                    result.Add(parameter.Name, parameter.Value);
                }
            }

            return result;
        }
    }
}