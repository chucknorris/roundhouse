
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

            if (!File.Exists(option)) throw new FileNotFoundException("File not found", option);

            var dictionary = new Dictionary<string, string>();
            using (var reader = File.OpenText(option))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if(String.IsNullOrEmpty(line)) continue;
                    var keyvalue = line.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    if(keyvalue.Length != 2) continue;
                    dictionary[keyvalue[0]] = keyvalue[1];
                }
            }
            return dictionary;
        }
    }
}