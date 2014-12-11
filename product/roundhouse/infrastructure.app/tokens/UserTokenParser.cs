
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
            if (String.IsNullOrEmpty(option)) throw new ArgumentNullException("option");
            
            var textToParse = option;
            var pairs = textToParse.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);

            if (pairs.Length == 1 && File.Exists(textToParse))
            {
                textToParse = File.ReadAllText(textToParse);
                pairs = textToParse.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
            }
            
            if (pairs.Any(p => !p.Contains("="))) throw new FormatException("Wrong format");

            return pairs.ToDictionary(p => p.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries)[0],
                p => p.Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1]);
        }
    }
}