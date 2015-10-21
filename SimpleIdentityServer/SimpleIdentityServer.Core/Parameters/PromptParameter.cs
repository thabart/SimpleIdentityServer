using System;

namespace SimpleIdentityServer.Core.Parameters
{
    [Flags]
    public enum PromptParameter
    {
        none,
        login,
        consent,
        select_account
    }
}
