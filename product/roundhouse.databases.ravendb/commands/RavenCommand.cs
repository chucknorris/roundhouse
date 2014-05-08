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
                    foreach (string header in CommandHeaders)
                    {
                        _webClient.Headers.Add(header);
                    }
                }

                switch (CommandType.ToUpper())
                {
                    case "PUT":
                    case "POST":
                    case "PATCH":
                    case "EVAL":
                        return _webClient.UploadString(CommandAddress, CommandType, CommandData);
                    case "DELETE":
                        return _webClient.UploadString(CommandAddress, CommandType, "");
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
                        if (CommandType.ToUpper() != "GET")
                        {
                            throw;
                        }
                        return null;
                    }
                }
                throw;
            }
        }

        public override string ToString()
        {
            return string.Format("CommandAddress: {1}{0}CommandData:{2}{0}CommandType:{3}{0}CommandHeaders:{4}{0}", Environment.NewLine, CommandAddress, CommandData, CommandType, CommandHeaders);
        }

        public static IRavenCommand CreateCommand(string connection_string, string script_to_run)
        {
            // parse script_to_run to be able to create the command
            string http_method = Regex.Match(script_to_run, @"^(?<httpmethod>\w*)").Value;
            Match match = Regex.Match(script_to_run, @"\s+https?:\/\/[^/]+(?<address>[^?\s]+)(?:\?(?<query>[^\s]*))?");
            string address = match.Groups["address"].Value;
            string query = match.Groups["query"].Value;
            string[] headers = Regex.Matches(script_to_run, @"\s+(?:-H|--header)\s+\""(?<headers>[^\""]*)\""").Cast<Match>()
                .Where(m => m.Success)
                .Select(m => m.Groups["headers"])
                .Select(g => g.Value)
                .ToArray();
            string data = Regex.Match(script_to_run, @"\s+(?:-d|--data)\s+""(?<data>(?:[^""\\]+|\\.)*)""").Groups["data"].Value;

            return CreateCommand(connection_string, address, query, http_method, headers, data);
        }

        public static IRavenCommand CreateCommand(string connection_string, string address, string query, string http_method, string[] headers, string data)
        {
            Match regexResult = Regex.Match(connection_string, connection_string_regex);

            if (!regexResult.Success) throw new ArgumentException("The connectionstring isn't a valid RavenDB connectionstring", "connection_string");

            string url = regexResult.Groups["url"].Value;
            string database = regexResult.Groups["database"].Value;

            var command_address = new UriBuilder(url);

            if (!string.IsNullOrEmpty(database))
            {
                command_address.Path += string.Format("databases/{0}", database);
            }

            command_address.Path += address;
            command_address.Query += query;

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