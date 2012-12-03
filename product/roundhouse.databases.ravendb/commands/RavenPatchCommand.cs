namespace roundhouse.databases.ravendb.commands
{
    public class RavenPatchCommand : RavenCommand
    {
        private readonly string _method;

        public RavenPatchCommand(string address, string data)
            : base(address)
        {
            Data = data;
            _method = "PATCH";
        }

        public string Data { get; protected set; }

        public override void ExecuteCommand()
        {
            WebClient.UploadString(Address, _method, Data);
        }
    }
}