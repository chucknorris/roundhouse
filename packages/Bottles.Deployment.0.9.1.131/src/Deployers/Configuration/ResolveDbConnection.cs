namespace Bottles.Deployment.Deployers.Configuration
{
    public class ResolveDbConnection : IDirective
    {
        public ResolveDbConnection()
        {
            File = "connectionStrings.config";
        }

        public string File { get; set; }
    }
}