using System;
namespace SimpleIdentityServer.Core.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        public static long ConvertToUnixTimestamp(this DateTimeOffset dateTime)
        {
            return (dateTime.ToUniversalTime().Ticks - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).Ticks) / 10000000L;
        }
    }
}
