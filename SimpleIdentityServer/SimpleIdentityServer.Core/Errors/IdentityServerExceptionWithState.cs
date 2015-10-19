namespace SimpleIdentityServer.Core.Errors
{
    public class IdentityServerExceptionWithState : IdentityServerException
    {
        public IdentityServerExceptionWithState(IdentityServerException ex, string state) : base(ex.Code, ex.Message, ex)
        {
            State = state;
        }

        public IdentityServerExceptionWithState(string code, string message, string state) : base(code, message)
        {
            State = state;
        }

        public string State { get; private set; }
    }
}
