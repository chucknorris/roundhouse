using System;

namespace FubuCore.Conversion
{
    public class TimeZoneConverter : StatelessConverter<TimeZoneInfo>
    {
        protected override TimeZoneInfo convert(string text)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(text);
        }
    }
}