using System;
using System.Collections.Generic;
using System.Linq;

namespace HtmlTags
{
    internal static class SharedExtensions
    {
        public static void Each<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T item in enumerable)
            {
                action(item);
            }
        }

        public static string ToFormat(this string template, params object[] parameters)
        {
            return string.Format(template, parameters);
        }

        public static string[] ToDelimitedArray(this string content, params char[] delimiter)
        {
            return content.Split(delimiter).Select(x => x.Trim()).ToArray();
        }

        public static string Join(this IEnumerable<string> strings, string separator)
        {
#if LEGACY
            return string.Join(separator, strings.ToArray());
#else
            return string.Join(separator, strings);
#endif
        }
    }
}