using System;
using System.IO;

namespace SimpleIdentityServer.Client.Test
{
    internal static class Logger
    {
        public static void Log(string logMessage, TextWriter w)
        {
            w.WriteLine($"Log Entry : {DateTime.UtcNow} : {logMessage}");
        }
    }
}
