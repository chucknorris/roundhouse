using System;
using System.Collections.Generic;

namespace FubuCore.CommandLine
{
    public static class StringTokenizer
    {
        public static IEnumerable<string> Tokenize(string content)
        {
            var searchString = content.Trim();
            if (searchString.Length == 0) return new string[0];

            var parser = new TokenParser();
            content.ToCharArray().Each(parser.Read);

            // Gotta force the parser to know it's done
            parser.Read('\n');

            return parser.Tokens;
        }
    }
}