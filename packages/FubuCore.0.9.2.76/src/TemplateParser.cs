using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FubuCore
{
    public interface IKeyValues
    {
        bool ContainsKey(string key);
        string Get(string key);
    }

    public class DictionaryKeyValues : IKeyValues
    {
        private readonly IDictionary<string, string> _dictionary;

        public DictionaryKeyValues(IDictionary<string, string> dictionary)
        {
            _dictionary = dictionary;
        }

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public string Get(string key)
        {
            return _dictionary[key];
        }
    }

    public static class TemplateParser
    {
        private static readonly string TemplateGroup;
        private static readonly Regex TemplateExpression;

        static TemplateParser()
        {
            TemplateGroup = "Template";
            TemplateExpression = new Regex(@"\{(?<" + TemplateGroup + @">[A-Za-z0-9_-]+)\}", RegexOptions.Compiled);
        }

        public static string Parse(string template, IDictionary<string, string> substitutions)
        {
            var values = new DictionaryKeyValues(substitutions);

            return Parse(template, values);
        }

        public static bool ContainsTemplates(string template)
        {
            return TemplateExpression.Matches(template).Count > 0;
        }

        public static IEnumerable<string> GetSubstitutions(string template)
        {
            var matches = TemplateExpression.Matches(template);
            foreach (Match match in matches)
            {
                yield return match.Groups[TemplateGroup].Value;
            }
        }

        public static string Parse(string template, IKeyValues values)
        {
            while(ContainsTemplates(template))
            {
                template = parse(template, values);
            }
            return template;
        }

        static string parse(string template, IKeyValues values)
        {
            var matches = TemplateExpression.Matches(template);
            if (matches.Count == 0) return template;

            var lastIndex = 0;
            var builder = new StringBuilder();
            foreach (Match match in matches)
            {
                var key = match.Groups[TemplateGroup].Value;
                if ((lastIndex == 0 || match.Index > lastIndex) && values.ContainsKey(key))
                {
                    builder.Append(template.Substring(lastIndex, match.Index - lastIndex));
                    builder.Append(values.Get(key));
                }

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < template.Length)
            {
                builder.Append(template.Substring(lastIndex, template.Length - lastIndex));
            }

            return builder.ToString();
        }
    }
}