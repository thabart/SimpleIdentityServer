namespace SimpleIdentityServer.Core.Results
{
    public enum TypeActionResult
    {
        RedirectToAction,
        RedirectToCallBackUrl,
        Output,
        None
    }

    public class ActionResult
    {
        public TypeActionResult Type { get; set; }

        public RedirectInstruction RedirectInstruction { get; set; }
    }
}
