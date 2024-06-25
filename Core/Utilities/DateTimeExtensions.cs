using TimeZoneConverter;

namespace Horde.Core.Utilities
{
    public static class CoreDateTimeExtensions
    {
        private static TimeZoneInfo _tzi = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        public static DateTime ToIst(this DateTime utc)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utc, _tzi);
        }
        public static DateTime FromUtc(this DateTime utc, string timezoneId)
        {
            var tzInfo = TZConvert.GetTimeZoneInfo(timezoneId);
            if (tzInfo == null)
                throw new ArgumentException($"Could not find timezone {timezoneId}. Valid IANA names are {TZConvert.KnownIanaTimeZoneNames}. Valid Windows names are {TZConvert.KnownWindowsTimeZoneIds}");

            return TimeZoneInfo.ConvertTimeFromUtc(utc, tzInfo);
        }
        public static DateTime ToUtc(this DateTime local, string timezoneId)
        {
            var source = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            return TimeZoneInfo.ConvertTimeToUtc(local, source);
        }

        public static string ToUnixTime(this DateTime dateTime)
        {
            DateTimeOffset dto = new DateTimeOffset(dateTime.ToUniversalTime());
            return dto.ToUnixTimeSeconds().ToString();
        }
    }
}
