namespace roundhouse.databases.ravendb.commands
{
    public class RavenDeleteCommand : RavenCommand
    {
        private readonly string _method;

        public RavenDeleteCommand(string address)
            : base(address)
        {
            _method = "DELETE";
        }
        
        public override string ExecuteCommand()
        {
            return WebClient.UploadString(Address, _method, null);
        }
    }
}