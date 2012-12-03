namespace roundhouse.databases.ravendb.commands
{
    public class RavenDeleteCommand : RavenCommand
    {
        public RavenDeleteCommand(string address) : base(address)
        {
        }

        public override void ExecuteCommand()
        {
            WebClient.DownloadString(Address);
        }
    }
}