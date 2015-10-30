using System.Web.Routing;

using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Api.Parsers
{
    public interface IActionResultParser
    {
        ActionInformation GetControllerAndActionFromRedirectionActionResult(ActionResult actionResult);

        RouteValueDictionary GetRedirectionParameters(ActionResult actionResult);
    }

    public class ActionResultParser : IActionResultParser
    {
        private readonly IRedirectInstructionParser _redirectInstructionParser;

        public ActionResultParser(IRedirectInstructionParser redirectInstructionParser)
        {
            _redirectInstructionParser = redirectInstructionParser;
        }

        public ActionInformation GetControllerAndActionFromRedirectionActionResult(ActionResult actionResult)
        {
            if (actionResult.Type != TypeActionResult.RedirectToAction 
                || actionResult.RedirectInstruction == null)
            {
                return null;
            }

            return _redirectInstructionParser.GetActionInformation(actionResult.RedirectInstruction);
        }

        public RouteValueDictionary GetRedirectionParameters(ActionResult actionResult)
        {
            if (actionResult.Type != TypeActionResult.RedirectToAction &&
                actionResult.Type != TypeActionResult.RedirectToCallBackUrl ||
                actionResult.RedirectInstruction == null)
            {
                return null;
            }

            return _redirectInstructionParser.GetRouteValueDictionary(actionResult.RedirectInstruction);
        }
    }
}