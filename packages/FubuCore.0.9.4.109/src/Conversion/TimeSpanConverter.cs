using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace FubuCore.Conversion
{
    public class TimeSpanConverter : StatelessConverter<TimeSpan>
    {
        private const string TIMESPAN_PATTERN =
            @"
(?<quantity>\d+     # quantity is expressed as some digits
(\.\d+)?)           # optionally followed by a decimal point and more digits
\s*                 # optional whitespace
(?<units>\w+)       # units is expressed as a word";


        public static TimeSpan GetTimeSpan(string timeString)
        {
            var match = Regex.Match(timeString, TIMESPAN_PATTERN, RegexOptions.IgnorePatternWhitespace);
            if (!match.Success)
            {
                return TimeSpan.Parse(timeString);
            }

            

            var number = double.Parse(match.Groups["quantity"].Value);
            var units = match.Groups["units"].Value.ToLower();
            switch (units)
            {
                case "s":
                case "second":
                case "seconds":
                    return TimeSpan.FromSeconds(number);
                case "m":
                case "minute":
                case "minutes":
                    return TimeSpan.FromMinutes(number);

                case "h":
                case "hour":
                case "hours":
                    return TimeSpan.FromHours(number);

                case "d":
                case "day":
                case "days":
                    return TimeSpan.FromDays(number);

            }

            if (timeString.Length == 4 && !timeString.Contains(":"))
            {
				int hours = int.Parse(timeString.Substring(0, 2));
				int minutes = int.Parse(timeString.Substring(2, 2));
				
				return new TimeSpan(hours, minutes, 0);
            }

            if (timeString.Length == 5 && timeString.Contains(":"))
            {
                var parts = timeString.Split(':');
                int hours = int.Parse(parts.ElementAt(0));
                int minutes = int.Parse(parts.ElementAt(1));

                return new TimeSpan(hours, minutes, 0);
            }

            throw new ApplicationException("Time periods must be expressed in seconds, minutes, hours, or days.");
        }

        protected override TimeSpan convert(string text)
        {
            return GetTimeSpan(text);
        }
    }
}