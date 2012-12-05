
using System.Net;

namespace roundhouse.databases.ravendb.commands
{
    public class RavenGetCommand : RavenCommand
    {
        private readonly string _method;

        public RavenGetCommand(string address)
            : base(address)
        {
            _method = "GET";
        }

        public override string ExecuteCommand()
        {
            try
            {
                return WebClient.DownloadString(Address);
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
    }
}