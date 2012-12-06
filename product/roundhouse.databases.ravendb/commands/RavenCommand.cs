using System;
using System.Net;
using System.Text.RegularExpressions;

namespace roundhouse.databases.ravendb.commands
{
    public sealed class RavenCommand : IRavenCommand
    {
        private const string script_to_run_regex = @"^(?<httpmethod>\w*)\s+https?:\/\/[^/]+(?<address>[^\s]+)(?:(?:\s+\-h\s+\""(?<headers>[^\""]*)\"")|(?:\s+\-d\s+\""(?<data>[^\""]*)\""))*";
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
        public string CommandHeaders { get; private set; }
        public string CommandData { get; private set; }
        public string CommandType { get; private set; }
        public int CommandTimeout { get; set; }

        public object Execute()
        {
            try
            {
                if (!string.IsNullOrEmpty(CommandHeaders))
                {
                    _webClient.Headers.Clear();
                    _webClient.Headers.Add(CommandHeaders);
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
            var regex_result = Regex.Match(script_to_run, script_to_run_regex, RegexOptions.Singleline);

            if (!regex_result.Success)
                throw new ArgumentException("The script cannot be mathed to a valid RavenCommand", "script_to_run");

            return CreateCommand(connection_string, regex_result.Groups["address"].Value, regex_result.Groups["httpmethod"].Value, regex_result.Groups["headers"].Value, regex_result.Groups["data"].Value);
        }

        public static IRavenCommand CreateCommand(string connection_string, string address, string http_method, string headers, string data)
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