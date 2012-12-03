
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
            return WebClient.DownloadString(Address);
        }
    }
}
