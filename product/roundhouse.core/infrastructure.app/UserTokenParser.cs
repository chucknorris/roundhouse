namespace roundhouse.infrastructure.app.tokens
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class UserTokenParser
    {
        public static Dictionary<string, string> Parse(string option)
        {
            if (String.IsNullOrEmpty(option))
                throw new ArgumentNullException("option");

            var textToParse = option;
            var pairStrings = textToParse.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            if (pairStrings.Length == 1 && File.Exists(textToParse))
            {
                textToParse = File.ReadAllText(textToParse);
                pairStrings = textToParse.Split(new string[] { ";",Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (pairStrings.Any(p => !p.Contains("=")))
                throw new FormatException("Wrong format");

            return pairStrings
                .Select(p => {
                    var split = p.Split('='); // Preserve the empties
                    return new KeyValuePair<string, string>(split.FirstOrDefault(), split.LastOrDefault());
                })
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
