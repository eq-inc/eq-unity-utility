using System;

namespace Eq.Unity
{
    public class TimeUtil
    {
        private static readonly DateTime UnixTimeBase = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static double UnixTime()
        {
            return (DateTimeOffset.UtcNow - UnixTimeBase).TotalSeconds;
        }

        public static double UnixTime(DateTimeOffset targetDateTime)
        {
            return (targetDateTime - UnixTimeBase).TotalSeconds;
        }

        public static long UnixTimeMilliseconds()
        {
            return (long)((DateTimeOffset.UtcNow - UnixTimeBase).TotalMilliseconds);
        }

        public static long UnixTimeMilliseconds(DateTimeOffset targetDateTime)
        {
            return (long)((targetDateTime - UnixTimeBase).TotalMilliseconds);
        }
    }
}
