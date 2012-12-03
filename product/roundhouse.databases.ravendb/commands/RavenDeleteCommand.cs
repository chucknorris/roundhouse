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
        
        public override void ExecuteCommand()
        {
            WebClient.UploadString(Address, _method, null);
        }
    }
}