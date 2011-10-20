using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Linq;

namespace FubuCore
{
    public static class StringExtensions
    {
        /// <summary>
        /// If the path is rooted, just returns the path.  Otherwise,
        /// combines root & path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        public static string CombineToPath(this string path, string root)
        {
            if (Path.IsPathRooted(path)) return path;

            return Path.Combine(root, path);
        }

        public static void IfNotNull(this string target, Action<string> continuation)
        {
            if (target != null)
            {
                continuation(target);
            }
        }

        public static string ToFullPath(this string path)
        {
            return Path.GetFullPath(path);
        } 

        /// <summary>
        /// Retrieve the parent directory of a directory or file
        /// Shortcut to Path.GetDirectoryName(path)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ParentDirectory(this string path)
        {
            return Path.GetDirectoryName(path);
        }

        /// <summary>
        /// Equivalent of FileSystem.Combine( [Union of path, parts] )
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parts"></param>
        /// <returns></returns>
        public static string AppendPath(this string path, params string[] parts)
        {
            var list = new List<string>{
                path
            };

            list.AddRange(parts);
            return FileSystem.Combine(list.ToArray());
        }

        public static string PathRelativeTo(this string path, string root)
        {
            var pathParts = path.getPathParts();
            var rootParts = root.getPathParts();

            var length = pathParts.Count > rootParts.Count ? rootParts.Count : pathParts.Count;
            for (int i = 0; i < length; i++)
            {
                if (pathParts.First() == rootParts.First())
                {
                    pathParts.RemoveAt(0);
                    rootParts.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }

            for (int i = 0; i < rootParts.Count; i++)
            {
                pathParts.Insert(0, "..");
            }            

            return pathParts.Count > 0 ? FileSystem.Combine(pathParts.ToArray()) : string.Empty;
        }

        public static bool IsEmpty(this string stringValue)
        {
            return string.IsNullOrEmpty(stringValue);
        }

        public static bool IsNotEmpty(this string stringValue)
        {
            return !string.IsNullOrEmpty(stringValue);
        }

        public static void IsNotEmpty(this string stringValue, Action<string> action)
        {
            if (stringValue.IsNotEmpty())
                action(stringValue);
        }

        public static bool ToBool(this string stringValue)
        {
            if (string.IsNullOrEmpty(stringValue)) return false;

            return bool.Parse(stringValue);
        }

        public static string ToFormat(this string stringFormat, params object[] args)
        {
            return String.Format(stringFormat, args);
        }

        /// <summary>
        /// Performs a case-insensitive comparison of strings
        /// </summary>
        public static bool EqualsIgnoreCase(this string thisString, string otherString)
        {
            return thisString.Equals(otherString, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Converts the string to Title Case
        /// </summary>
        public static string Capitalize(this string stringValue)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(stringValue);
        }

        public static string HtmlAttributeEncode(this string unEncoded)
        {
            return HttpUtility.HtmlAttributeEncode(unEncoded);
        }

        public static string HtmlEncode(this string unEncoded)
        {
            return HttpUtility.HtmlEncode(unEncoded);
        }

        public static string HtmlDecode(this string encoded)
        {
            return HttpUtility.HtmlDecode(encoded);
        }

        public static string UrlEncode(this string unEncoded)
        {
            return HttpUtility.UrlEncode(unEncoded);
        }

        public static string UrlDecode(this string encoded)
        {
            return HttpUtility.UrlDecode(encoded);
        }

        /// <summary>
        /// Formats a multi-line string for display on the web
        /// </summary>
        /// <param name="plainText"></param>
        public static string ConvertCRLFToBreaks(this string plainText)
        {
            return new Regex("(\r\n|\n)").Replace(plainText, "<br/>");
        }

        /// <summary>
        /// Returns a DateTime value parsed from the <paramref name="dateTimeValue"/> parameter.
        /// </summary>
        /// <param name="dateTimeValue">A valid, parseable DateTime value</param>
        /// <returns>The parsed DateTime value</returns>
        public static DateTime ToDateTime(this string dateTimeValue)
        {
            return DateTime.Parse(dateTimeValue);
        }

        public static string ToGmtFormattedDate(this DateTime date)
        {
            return date.ToString("yyyy'-'MM'-'dd hh':'mm':'ss tt 'GMT'");
        }

        public static string[] ToDelimitedArray(this string content)
        {
            return content.ToDelimitedArray(',');
        }

        public static string[] ToDelimitedArray(this string content, char delimiter)
        {
            string[] array = content.Split(delimiter);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = array[i].Trim();
            }

            return array;
        }

        public static bool IsValidNumber(this string number)
        {
            return IsValidNumber(number, Thread.CurrentThread.CurrentCulture);
        }

        public static bool IsValidNumber(this string number, CultureInfo culture)
        {
            string _validNumberPattern =
            @"^-?(?:\d+|\d{1,3}(?:" 
            + culture.NumberFormat.NumberGroupSeparator + 
            @"\d{3})+)?(?:\" 
            + culture.NumberFormat.NumberDecimalSeparator + 
            @"\d+)?$";

            return new Regex(_validNumberPattern, RegexOptions.ECMAScript).IsMatch(number);
        }

        public static IList<string> getPathParts(this string path)
        {
            return path.Split(new[] {Path.DirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
		
		public static string DirectoryPath(this string path)
		{
			return Path.GetDirectoryName(path);
		}

        /// <summary>
        /// Reads text and returns an enumerable of strings for each line
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IEnumerable<string> ReadLines(this string text)
        {
            var reader = new StringReader(text);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        /// <summary>
        /// Reads text and calls back for each line of text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static void ReadLines(this string text, Action<string> callback)
        {
            var reader = new StringReader(text);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                callback(line);
            }
        }
    }
}