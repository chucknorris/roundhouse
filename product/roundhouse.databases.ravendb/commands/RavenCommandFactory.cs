using System;
using System.Text.RegularExpressions;

namespace roundhouse.databases.ravendb.commands
{
    public interface IRavenCommandFactory
    {
        IRavenCommand CreateRavenCommand(string command);

        string ConnectionString { get; set; }
    }

    public class RavenCommandFactory
    : IRavenCommandFactory
    {
        private string regex =
            @"(?<httpmethod>.*)(?<spaces>\s+)(?<address>http.*)(?<spaces>\s\x2d[d]{1}\s+)\""(?<data>.*)\""\s*$";

        public string ConnectionString { get; set; }

        public IRavenCommand CreateRavenCommand(string command)
        {
            Match result = Regex.Match(command, regex, RegexOptions.Multiline);
            string httpMethod = string.Empty;
            string address = string.Empty;
            string data = string.Empty;
            if (result.Success)
            {
                httpMethod = result.Groups["httpmethod"].Value.Trim();
                address = result.Groups["address"].Value.Trim();
                data = result.Groups["data"].Value.Trim();
            }

            if (!String.IsNullOrWhiteSpace(ConnectionString))
            {
                ConnectionString = ConnectionString.TrimEnd('/');
                Match replaceMatch = Regex.Match(address, @"^((http[s]?|ftp):\/)?\/?(?<domain>[^\/\s]+)(?<remaining>.*)$");
                string lastPartUri;
                if (replaceMatch.Success)
                {
                    lastPartUri= replaceMatch.Groups["remaining"].Value;
                    address = ConnectionString + lastPartUri;
                }
            }

            switch (httpMethod.ToUpper())
            {
                case "DELETE":
                    return new RavenDeleteCommand(address);
                case "POST":
                    return new RavenPostCommand(address, data);
                case "PATCH":
                    return new RavenPatchCommand(address, data);
                case "PUT":
                    return new RavenPutCommand(address, data);
                case "GET":
                    return new RavenGetCommand(address);
                default:
                    throw new NotSupportedException(string.Format("HttpMethod {0} is not supported", httpMethod));
            }
        }
    }
}