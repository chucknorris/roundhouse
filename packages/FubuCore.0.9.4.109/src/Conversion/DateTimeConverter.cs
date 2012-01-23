using System;
using System.Linq;

namespace FubuCore.Conversion
{
    public class DateTimeConverter : StatelessConverter<DateTime>
    {
        public const string TODAY = "TODAY";

        protected override DateTime convert(string text)
        {
            return GetDateTime(text);
        }

        public static DateTime GetDateTime(string dateString)
        {
            var trimmedString = dateString.Trim();
            if (trimmedString == TODAY)
            {
                return DateTime.Today;
            }

            if (trimmedString.Contains(TODAY))
            {
                var dayString = trimmedString.Substring(5, trimmedString.Length - 5);
                var days = int.Parse(dayString);

                return DateTime.Today.AddDays(days);
            }

            if (isDayOfWeek(dateString))
            {
                return convertToDateFromDayAndTime(dateString);
            }

            return DateTime.Parse(trimmedString);
        }

        private static DateTime convertToDateFromDayAndTime(string dateString)
        {
            dateString = dateString.Replace("  ", " ");
            var parts = dateString.Split(' ');
            var day = (DayOfWeek) Enum.Parse(typeof (DayOfWeek), parts[0], true);
            var minutes = minutesFrom24HourTime(parts[1]);

            var date = DateTime.Today.AddMinutes(minutes);
            while (date.DayOfWeek != day)
            {
                date = date.AddDays(1);
            }

            return date;
        }

        private static bool isDayOfWeek(string text)
        {
            var days = Enum.GetNames(typeof (DayOfWeek));
            return days.FirstOrDefault(x => text.ToLower().StartsWith(x.ToLower())) != null;
        }


        private static int minutesFrom24HourTime(string time)
        {
            var parts = time.Split(':');
            return 60*int.Parse(parts[0]) + int.Parse(parts[1]);
        }
    }
}