using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp
{
    internal static class Utils
    {
        private static DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long GetTimeNow()
        {
            return (long) (DateTime.UtcNow - startTime).TotalSeconds;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            return startTime.AddSeconds(unixTimeStamp).ToLocalTime();
        }
    }
}
