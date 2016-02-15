namespace SimpleIdentityServer.Host.Parsers
{
    public static class ActionResultParserFactory
    {
        public static IActionResultParser CreateActionResultParser()
        {
            return new ActionResultParser(new RedirectInstructionParser());
        }
    }
}