using System;
using System.Text.RegularExpressions;

namespace roundhouse.databases.ravendb.commands
{
    public interface IRavenCommandFactory
    {
        IRavenCommand CreateRavenCommand(string command);
    }

    public class RavenCommandFactory
    : IRavenCommandFactory
    {

        private string regex =
            @"(?<httpmethod>.*)(?<spaces>\s+)(?<address>http.*)(?<spaces>\s\x2d[d]{1}\s+)\""(?<data>.*)\""\s*$";

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
                default:
                    throw new NotSupportedException(string.Format("HttpMethod {0} is not supported", httpMethod));
            }
        }
    }
}