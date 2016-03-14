using System.Collections.Generic;
using System.Linq;
using SimpleIdentityServer.Core.Results;
using Microsoft.AspNet.Routing;

namespace SimpleIdentityServer.Host.Parsers
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
            },
            {
                IdentityServerEndPoints.FormIndex,
                new ActionInformation("Form", "Index")
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