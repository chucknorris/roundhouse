using System;
using System.Text.RegularExpressions;

namespace roundhouse.databases.ravendb.commands
{
    public interface IRavenCommandFactory
    {
        IRavenCommand CreateRavenCommand(string command);

        string ConnectionString { get; set; }
    }

    public class RavenCommandFactory : IRavenCommandFactory
    {
        private string regex = @"^(?<httpmethod>\w*)\s+(?<address>https?:\/\/[^\s]+)(?:(?:\s+\-h\s+\""(?<headers>[^\""]*)\"")|(?:\s+\-d\s+\""(?<data>[^\""]*)\""))*";

        public string ConnectionString { get; set; }

        public IRavenCommand CreateRavenCommand(string command)
        {
            Match result = Regex.Match(command, regex, RegexOptions.Singleline);
            var httpMethod = string.Empty;
            var address = string.Empty;
            var headers = string.Empty;
            var data = string.Empty;

            if (result.Success)
            {
                httpMethod = result.Groups["httpmethod"].Value.Trim();
                address = result.Groups["address"].Value.Trim();
                headers = result.Groups["headers"].Value.Trim();
                data = result.Groups["data"].Value.Trim();
            }

            if (!String.IsNullOrEmpty(ConnectionString))
            {
                ConnectionString = ConnectionString.TrimEnd('/');
                Match replaceMatch = Regex.Match(address, @"^https?:\/\/(?<domain>[^\/\s]+)(?<remaining>.*)$");
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