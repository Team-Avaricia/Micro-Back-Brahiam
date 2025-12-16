using System;

namespace Core.Domain.Common
{
    /// <summary>
    /// Helper class for handling Colombia timezone conversions.
    /// Colombia uses UTC-5 (SA Pacific Standard Time) without daylight saving.
    /// </summary>
    public static class ColombiaTimeZone
    {
        private static readonly TimeZoneInfo _colombiaZone;

        static ColombiaTimeZone()
        {
            try
            {
                // Windows uses "SA Pacific Standard Time" for Colombia
                _colombiaZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            }
            catch
            {
                try
                {
                    // Linux/macOS uses "America/Bogota"
                    _colombiaZone = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
                }
                catch
                {
                    // Fallback: Create a custom timezone for UTC-5 (Colombia)
                    _colombiaZone = TimeZoneInfo.CreateCustomTimeZone(
                        "Colombia Standard Time",
                        TimeSpan.FromHours(-5),
                        "Colombia Standard Time",
                        "Colombia Standard Time"
                    );
                }
            }
        }

        /// <summary>
        /// Gets the current time in Colombia timezone.
        /// </summary>
        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _colombiaZone);

        /// <summary>
        /// Converts a UTC DateTime to Colombia timezone.
        /// </summary>
        public static DateTime FromUtc(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _colombiaZone);
        }

        /// <summary>
        /// Converts a Colombia local DateTime to UTC.
        /// </summary>
        public static DateTime ToUtc(DateTime colombiaDateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(colombiaDateTime, _colombiaZone);
        }
    }
}
