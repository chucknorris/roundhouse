using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace roundhouse.databases.ravendb.commands
{
    public sealed class RavenCommand : IRavenCommand
    {
        private const string connection_string_regex = @"(?:(?:Url=(?<url>[^;]*)[;]?)|(?:Database=(?<database>[^;]*)[;]?))*";

        private readonly WebClient _webClient;

        public RavenCommand()
        {
            _webClient = new WebClient();
        }

        public void Dispose()
        {
            _webClient.Dispose();
        }

        public Uri CommandAddress { get; private set; }
        public string[] CommandHeaders { get; private set; }
        public string CommandData { get; private set; }
        public string CommandType { get; private set; }
        public int CommandTimeout { get; set; }

        public object Execute()
        {
            try
            {
                if (CommandHeaders != null)
                {
                    _webClient.Headers.Clear();
                    foreach (var header in CommandHeaders)
                    {
                        _webClient.Headers.Add(header);
                    }
                }

                switch (CommandType.ToUpper())
                {
                    case "PUT":
                    case "POST":
                    case "PATCH":
                        return _webClient.UploadString(CommandAddress, CommandType, CommandData);
                    case "DELETE":
                        return _webClient.UploadString(CommandAddress, CommandType, null);
                    case "GET":
                        return _webClient.DownloadString(CommandAddress);
                    default:
                        throw new NotSupportedException(string.Format("HttpMethod {0} is not supported", CommandType));
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    var resp = (HttpWebResponse) ex.Response;

                    if (resp.StatusCode == HttpStatusCode.NotFound) // HTTP 404
                    {
                        return null;
                    }
                }

                throw;
            }
        }

        public static IRavenCommand CreateCommand(string connection_string, string script_to_run)
        {
            // parse script_to_run to be able to create the command
            var http_method = Regex.Match(script_to_run, @"^(?<httpmethod>\w*)").Value;
            var address = Regex.Match(script_to_run, @"\s+https?:\/\/[^/]+(?<address>[^\s]+)").Groups["address"].Value;
            var headers = Regex.Matches(script_to_run, @"\s+(?:-H|--header)\s+\""(?<headers>[^\""]*)\""").Cast<Match>()
                               .Where(m => m.Success)
                               .Select(m => m.Groups["headers"])
                               .Select(g => g.Value)
                               .ToArray();
            var data = Regex.Match(script_to_run, @"\s+(?:-d|--data)\s+""(?<data>(?:[^""\\]+|\\.)*)""").Groups["data"].Value;

            return CreateCommand(connection_string, address, http_method, headers, data);
        }

        public static IRavenCommand CreateCommand(string connection_string, string address, string http_method, string[] headers, string data)
        {
            var regex_result = Regex.Match(connection_string, connection_string_regex);

            if (!regex_result.Success)
                throw new ArgumentException("The connectionstring isn't a valid RavenDB connectionstring", "connection_string");

            var url = regex_result.Groups["url"].Value;
            var database = regex_result.Groups["database"].Value;

            var command_address = new UriBuilder(url);

            if (!string.IsNullOrEmpty(database))
            {
                command_address.Path += string.Format("databases/{0}", database);
            }

            command_address.Path += address;

            var ravenCommand = new RavenCommand
                {
                    CommandType = http_method,
                    CommandAddress = command_address.Uri,
                    CommandHeaders = headers,
                    CommandData = data
                };

            return ravenCommand;
        }
    }
}