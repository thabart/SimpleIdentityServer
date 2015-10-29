using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Api.Parsers
{
    public interface IActionResultParser
    {
        ActionInformation GetControllerAndActionFromRedirectionActionResult(ActionResult actionResult);
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
            if (actionResult.Type != TypeActionResult.Redirection 
                || actionResult.RedirectInstruction == null)
            {
                return null;
            }

            return _redirectInstructionParser.GetActionInformation(actionResult.RedirectInstruction);
        }
    }
}