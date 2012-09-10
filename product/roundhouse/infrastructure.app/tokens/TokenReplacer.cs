namespace roundhouse.infrastructure.app.tokens
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using extensions;

    public class TokenReplacer
    {
        public static string replace_tokens(ConfigurationPropertyHolder configuration, string text_to_replace)
        {
            if (string.IsNullOrEmpty(text_to_replace)) return string.Empty;

            IDictionary<string, string> dictionary = create_dictionary_from_configuration(configuration);
            Regex regex = new Regex("{{(?<key>\\w+)}}");

            string output = regex.Replace(text_to_replace, m =>
            {
                string key = "";

                key = m.Groups["key"].Value;
                if (!dictionary.ContainsKey(key))
                {
                    return "{{" + key + "}}";
                }

                var value = dictionary[key];
                return value;
            });

            return output;
        }

        private static IDictionary<string, string> create_dictionary_from_configuration(ConfigurationPropertyHolder configuration)
        {
            Dictionary<string, string> property_dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in configuration.GetType().GetProperties())
            {
                property_dictionary.Add(property.Name, property.GetValue(configuration, null).to_string());
            }

            return property_dictionary;
        }
    }
}