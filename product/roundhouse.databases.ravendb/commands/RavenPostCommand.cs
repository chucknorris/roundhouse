namespace roundhouse.databases.ravendb.commands
{
    public class RavenPostCommand : RavenCommand
    {
        private readonly string _method;

        public RavenPostCommand(string address, string data) : base(address)
        {
            Data = data;
            _method = "POST";
        }

        public string Data { get; protected set; }

        public override void ExecuteCommand()
        {
            WebClient.UploadString(Address, _method, Data);
        }
    }
}