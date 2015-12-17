namespace SimpleIdentityServer.Api.Parsers
{
    public static class ActionResultParserFactory
    {
        public static IActionResultParser CreateActionResultParser()
        {
            return new ActionResultParser(new RedirectInstructionParser());
        }
    }
}