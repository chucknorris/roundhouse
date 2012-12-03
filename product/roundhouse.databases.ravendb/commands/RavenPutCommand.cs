namespace roundhouse.databases.ravendb.commands
{
    public class RavenPutCommand : RavenCommand
    {
        private readonly string _method;

        public RavenPutCommand(string address, string data) : base(address)
        {
            Data = data;
            _method = "PUT";
        }

        public string Data { get; protected set; }

        public override void ExecuteCommand()
        {
            WebClient.UploadString(Address, _method, Data);
        }
    }
}