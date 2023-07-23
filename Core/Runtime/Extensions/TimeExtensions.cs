using System;
using Common.Core.Logs;

namespace Common.Core
{
    public static class TimeExtensions
    {
        public static readonly DateTime EpochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static readonly DateTime MaxUnixDateTime = EpochTime.AddSeconds(int.MaxValue);

        public static long ToTimestamp(this DateTime dateTime)
        {
            if (dateTime < EpochTime)
            {
                Log.Warning($"{dateTime} is less than Epoch Time");
                return 0;
            }

            if (dateTime > MaxUnixDateTime)
            {
                Log.Warning($"{dateTime} is greater than max unix time: {MaxUnixDateTime}");
                dateTime = MaxUnixDateTime;
            }

            return (long) dateTime.Subtract(EpochTime).TotalSeconds;
        }
    }
}