using System;

namespace SimpleIdentityServer.Scim.Db.EF.Extensions
{
    public static class DateTimeExtensions
    {
        public static double ToUnix(this DateTime dateTime)
        {
            var epochTicks = new TimeSpan(new DateTime(1970, 1, 1).Ticks);
            TimeSpan unixTicks = new TimeSpan(dateTime.Ticks) - epochTicks;
            return unixTicks.TotalSeconds;
        }

        public static DateTime ToDateTime(this double unixTime)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            long unixTimeStampInTicks = (long)(unixTime * TimeSpan.TicksPerSecond);
            return new DateTime(unixStart.Ticks + unixTimeStampInTicks, System.DateTimeKind.Utc);
        }
    }
}
