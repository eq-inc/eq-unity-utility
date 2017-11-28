using System;

namespace Eq.Unity
{
    public class TimeUtil
    {
        private static readonly DateTime UnixTimeBase = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static double UnixTime()
        {
            return (DateTime.Now - UnixTimeBase).TotalSeconds;
        }

        public static double UnixTime(DateTime targetDateTime)
        {
            return (targetDateTime - UnixTimeBase).TotalSeconds;
        }

        public static long UnixTimeMilliseconds()
        {
            return (long)((DateTime.Now - UnixTimeBase).TotalMilliseconds);
        }

        public static long UnixTimeMilliseconds(DateTime targetDateTime)
        {
            return (long)((targetDateTime - UnixTimeBase).TotalMilliseconds);
        }
    }
}
