using System.Collections.Generic;

namespace SimpleIdentityServer.Module
{
    public static class OptionsExtensions
    {
        public static string TryGetValue(this IDictionary<string, string> opts, string name)
        {
            if (!opts.ContainsKey(name))
            {
                return null;
            }

            return opts[name];
        }

        public static bool TryGetValue(this IDictionary<string, string> opts, string name, out int val)
        {
            val = 0;
            if (!opts.ContainsKey(name))
            {
                return false;
            }

            return int.TryParse(opts[name], out val);
        }

        public static bool TryGetValue(this IDictionary<string, string> opts, string name, out bool val)
        {
            val = false;
            if (!opts.ContainsKey(name))
            {
                return false;
            }

            return bool.TryParse(opts[name], out val);
        }

        public static IEnumerable<string> TryGetArr(this IDictionary<string, string> opts, string name)
        {
            if (!opts.ContainsKey(name))
            {
                return new string[0];
            }

            return opts[name].Split(',');
        }
    }
}
