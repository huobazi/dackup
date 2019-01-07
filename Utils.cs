using System;

namespace dackup
{
    public static class Utils
    {
        public static TimeSpan ConvertRemoveThresholdToTimeSpan(string timeSpan)
        {
            if (timeSpan.Length < 2)
            {
                throw new InvalidOperationException($"Invalid value for option: remove_threshold '{timeSpan}'");
            }

            var l = timeSpan.Length - 1;
            var value = timeSpan.Substring(0, l);
            var type = timeSpan.Substring(l, 1);

            switch (type)
            {
                case "d": return TimeSpan.FromDays(double.Parse(value));
                case "h": return TimeSpan.FromHours(double.Parse(value));
                case "m": return TimeSpan.FromMinutes(double.Parse(value));
                case "s": return TimeSpan.FromSeconds(double.Parse(value));
                case "f": return TimeSpan.FromMilliseconds(double.Parse(value));
                case "z": return TimeSpan.FromTicks(long.Parse(value));
                default: throw new InvalidOperationException($"Invalid value for remove_threshold option: '{timeSpan}'");
            }
        }

        public static DateTime ConvertRemoveThresholdToDateTime(string timeSpan)
        {
            return DateTime.Now - ConvertRemoveThresholdToTimeSpan(timeSpan);
        }

    }
}