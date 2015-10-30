using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Core.Factories
{
    public interface IActionResultFactory
    {
        ActionResult CreateAnEmptyActionResultWithRedirectionToCallBackUrl();

        ActionResult CreateAnEmptyActionResultWithRedirection();

        ActionResult CreateAnEmptyActionResultWithOutput();

        ActionResult CreateAnEmptyActionResultWithNoEffect();
    }

    public class ActionResultFactory : IActionResultFactory
    {
        /// <summary>
        /// Creates an empty action result with redirection
        /// </summary>
        /// <returns>Empty action result with redirection</returns>
        public ActionResult CreateAnEmptyActionResultWithRedirection()
        {
            return new ActionResult
            {
                RedirectInstruction = new RedirectInstruction(),
                Type = TypeActionResult.RedirectToAction
            };
        }

        /// <summary>
        /// Creates an empty action result with output
        /// </summary>
        /// <returns>Empty action result with output</returns>
        public ActionResult CreateAnEmptyActionResultWithOutput()
        {
            return new ActionResult
            {
                RedirectInstruction = null,
                Type = TypeActionResult.Output
            };
        }

        /// <summary>
        /// Creates an empty action result with no effect
        /// </summary>
        /// <returns>Empty action result with no effect</returns>
        public ActionResult CreateAnEmptyActionResultWithNoEffect()
        {
            return new ActionResult
            {
                Type = TypeActionResult.None
            };
        }

        /// <summary>
        /// Creates an empty action result with redirection to callbackurl.
        /// </summary>
        /// <returns>Empty action with redirection to callbackurl</returns>
        public ActionResult CreateAnEmptyActionResultWithRedirectionToCallBackUrl()
        {
            return new ActionResult
            {
                Type = TypeActionResult.RedirectToCallBackUrl,
                RedirectInstruction = new RedirectInstruction()
            };
        }
    }
}
