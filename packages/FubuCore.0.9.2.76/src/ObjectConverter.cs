using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using FubuCore.Reflection.Expressions;
using FubuCore.Util;

namespace FubuCore
{
    public interface IObjectConverter
    {
        /// <summary>
        /// Given a string and a .Net type, read this string
        /// and give me back a corresponding instance of that
        /// type
        /// </summary>
        /// <param name="stringValue">The value to convert</param>
        /// <param name="type">The desired destination type</param>
        /// <returns>The value converted into the specified desination type</returns>
        object FromString(string stringValue, Type type);
        /// <summary>
        /// Given a string and a .Net type, T, read this string
        /// and give me back a corresponding instance of type T.
        /// </summary>
        /// <typeparam name="T">The desired destination type</typeparam>
        /// <param name="stringValue">The value to convert</param>
        /// <returns>The value converted into the specified desination type</returns>
        T FromString<T>(string stringValue);
        /// <summary>
        /// Determines whether there is conversion support registered for the specified type
        /// </summary>
        /// <param name="type">The desired destination type</param>
        /// <returns>True if conversion to this type is supported, otherwise False.</returns>
        bool CanBeParsed(Type type);
    }

    public interface IObjectConverterFamily
    {
        // ObjectConverter calls this method on unknown types
        // to ask an IObjectConverterFamily if it "knows" how
        // to convert a string into the given type
        bool Matches(Type type, IObjectConverter converter);

        // If Matches() returns true for a given type, 
        // ObjectConverter asks this IObjectConverterFamily
        // for a converter Lambda and calls its 
        // RegisterFinder() method behind the scenes to cache
        // the Lambda for later usage
        Func<string, object> CreateConverter(Type type, Cache<Type, Func<string, object>> converters);
    }

    public class ObjectConverter : IObjectConverter
    {
        public const string EMPTY = "EMPTY";
        public const string NULL = "NULL";
        public const string BLANK = "BLANK";

        private const string TIMESPAN_PATTERN =
            @"
(?<quantity>\d+     # quantity is expressed as some digits
(\.\d+)?)           # optionally followed by a decimal point and more digits
\s*                 # optional whitespace
(?<units>\w+)       # units is expressed as a word";

        public const string TODAY = "TODAY";

        private readonly Cache<Type, Func<string, object>> _froms;
        private readonly IList<IObjectConverterFamily> _families = new List<IObjectConverterFamily>();

        public ObjectConverter()
        {
            _froms = new Cache<Type, Func<string, object>>(createFinder);
            Clear();
        }

        private Func<string, object> createFinder(Type type)
        {
            var family = _families.FirstOrDefault(x => x.Matches(type, this));
            if (family != null)
            {
                return family.CreateConverter(type, _froms);
            }

            // TODO -- change to using the TypeDescriptor stuff
            return stringValue => Convert.ChangeType(stringValue, type);
        }

        public bool CanBeParsed(Type type)
        {
            return _froms.Has(type) || _families.Any(x => x.Matches(type, this));
        }

        public void RegisterConverter<T>(Func<string, T> finder)
        {
            _froms[typeof(T)] = x => finder(x);
        }

        public void RegisterConverterFamily(IObjectConverterFamily family)
        {
            _families.Insert(0, family);
        }

        public void Clear()
        {
            _froms.ClearAll();
            _froms[typeof(string)] = parseString;
            _froms[typeof(DateTime)] = key => GetDateTime(key);
            _froms[typeof(TimeSpan)] = key => GetTimeSpan(key);
            _froms[typeof (TimeZoneInfo)] = key => TimeZoneInfo.FindSystemTimeZoneById(key);

            _families.Clear();
            _families.Add(new EnumConverterFamily());
            _families.Add(new ArrayConverterFamily());
            _families.Add(new NullableConverterFamily());
            _families.Add(new TypeDescriptorFamily());
            _families.Add(new StringConstructorConverterFamily());
        }

        public virtual object FromString(string stringValue, Type type)
        {
            return stringValue == NULL ? null : _froms[type](stringValue);
        }

        public virtual T FromString<T>(string stringValue)
        {
            return (T)FromString(stringValue, typeof(T));
        }

        private static string parseString(string stringValue)
        {
            if (stringValue == BLANK || stringValue == EMPTY)
            {
                return string.Empty;
            }

            if (stringValue == NULL)
            {
                return null;
            }

            return stringValue;
        }

        public static DateTime GetDateTime(string dateString)
        {
            string trimmedString = dateString.Trim();
            if (trimmedString == TODAY)
            {
                return DateTime.Today;
            }

            if (trimmedString.Contains(TODAY))
            {
                string dayString = trimmedString.Substring(5, trimmedString.Length - 5);
                int days = int.Parse(dayString);

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
            string[] parts = dateString.Split(' ');
            var day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), parts[0], true);
            int minutes = minutesFrom24HourTime(parts[1]);

            DateTime date = DateTime.Today.AddMinutes(minutes);
            while (date.DayOfWeek != day)
            {
                date = date.AddDays(1);
            }

            return date;
        }

        private static bool isDayOfWeek(string text)
        {
            string[] days = Enum.GetNames(typeof(DayOfWeek));
            return days.FirstOrDefault(x => text.ToLower().StartsWith(x.ToLower())) != null;
        }

        private static int minutesFrom24HourTime(string time)
        {
            string[] parts = time.Split(':');
            return 60 * int.Parse(parts[0]) + int.Parse(parts[1]);
        }

        public static TimeSpan GetTimeSpan(string timeString)
        {
            Match match = Regex.Match(timeString, TIMESPAN_PATTERN, RegexOptions.IgnorePatternWhitespace);
            if (!match.Success)
            {
                return TimeSpan.Parse(timeString);
            }

            double number = double.Parse(match.Groups["quantity"].Value);
            string units = match.Groups["units"].Value.ToLower();
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

            throw new ApplicationException("Time periods must be expressed in seconds, minutes, hours, or days.");
        }
    }

    public class EnumConverterFamily : IObjectConverterFamily
    {
        public bool Matches(Type type, IObjectConverter converter)
        {
            return type.IsEnum;
        }

        public Func<string, object> CreateConverter(Type type, Cache<Type, Func<string, object>> converters)
        {
            return stringValue => Enum.Parse(type, stringValue);
        }
    }

    public class NullableConverterFamily : IObjectConverterFamily
    {
        public bool Matches(Type type, IObjectConverter converter)
        {
            return type.IsNullable();
        }

        public Func<string, object> CreateConverter(Type type, Cache<Type, Func<string, object>> converters)
        {
            Func<string, object> inner = converters[type.GetInnerTypeFromNullable()];

            return stringValue =>
            {
                if (stringValue == ObjectConverter.NULL || stringValue == null) return null;
                if (stringValue == string.Empty && type != typeof(string)) return null;

                return inner(stringValue);
            };
        }
    }

    public class TypeDescriptorFamily : IObjectConverterFamily
    {
        public bool Matches(Type type, IObjectConverter converter)
        {
            return TypeDescriptor.GetConverter(type).CanConvertFrom(typeof (string));
        }

        public Func<string, object> CreateConverter(Type type, Cache<Type, Func<string, object>> converters)
        {
            var converter = TypeDescriptor.GetConverter(type);
            return s => converter.ConvertFromString(s);
        }
    }

    public class ArrayConverterFamily : IObjectConverterFamily
    {
        public bool Matches(Type type, IObjectConverter converter)
        {
            if (type.IsArray && converter.CanBeParsed(type.GetElementType())) return true;




            return (type.IsGenericEnumerable() && converter.CanBeParsed(type.GetGenericArguments()[0]));
        }

        public Func<string, object> CreateConverter(Type type, Cache<Type, Func<string, object>> converters)
        {
            var innerType = type.IsGenericEnumerable() ? type.GetGenericArguments()[0] : type.GetElementType();

            var singleObjectFinder = converters[innerType];

            return stringValue =>
            {
                if (stringValue.ToUpper() == ObjectConverter.EMPTY)
                {
                    return Array.CreateInstance(innerType, 0);
                }

                var strings = stringValue.ToDelimitedArray();
                var array = Array.CreateInstance(innerType, strings.Length);

                for (var i = 0; i < strings.Length; i++)
                {
                    var value = singleObjectFinder(strings[i]);
                    array.SetValue(value, i);
                }

                return array;
            };
        }
    }

    public class StringConstructorConverterFamily : IObjectConverterFamily
    {
        public bool Matches(Type type, IObjectConverter converter)
        {
            if (type.IsArray) return false;

            var constructorInfo = type.GetConstructor(new Type[]{typeof (string)});
            return constructorInfo != null;
        }

        public Func<string, object> CreateConverter(Type type, Cache<Type, Func<string, object>> converters)
        {
            var builder = typeof (FuncBuilder<>).CloseAndBuildAs<FuncBuilder>(type);
            return builder.Build;
        }

        public interface FuncBuilder
        {
            object Build(string value);
        }

        public class FuncBuilder<T> : FuncBuilder
        {
            private Func<string, T> _func;

            public FuncBuilder()
            {
                _func = ConstructorBuilder.CreateSingleStringArgumentConstructor(typeof (T))
                    .Compile()
                    .As<Func<string, T>>();
            }

            public object Build(string value)
            {
                return _func(value);
            }
        }
    }


}