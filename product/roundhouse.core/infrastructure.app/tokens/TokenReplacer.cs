namespace roundhouse.infrastructure.app.tokens
{
    using System.Text.RegularExpressions;

    public class TokenReplacer
    {
        public static string replace_tokens(ConfigurationPropertyHolder configuration, string text_to_replace)
        {
            if (string.IsNullOrEmpty(text_to_replace)) return string.Empty;

            var dictionary = configuration.to_token_dictionary();
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
    }
}