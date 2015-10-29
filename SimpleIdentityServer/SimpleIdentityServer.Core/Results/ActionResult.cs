namespace SimpleIdentityServer.Core.Results
{
    public enum TypeActionResult
    {
        Redirection,
        Output,
        None
    }

    public class ActionResult
    {
        public TypeActionResult Type { get; set; }

        public RedirectInstruction RedirectInstruction { get; set; }
    }
}
