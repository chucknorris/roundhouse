using System.Linq;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Deployment.Deployers.Configuration
{
    public class ResolveDbConnectionDeployer : IDeployer<ResolveDbConnection>
    {
        private readonly IConnectionStringResolver _resolver;
        public static string COULD_NOT_FIND_DIRECTIVE_PATH = "Could not find an IDirectiveWithRoot to determine the application path for this host";

        public ResolveDbConnectionDeployer(IConnectionStringResolver resolver)
        {
            _resolver = resolver;
        }

        public void Execute(ResolveDbConnection directive, HostManifest host, IPackageLog log)
        {
            var pathedDirective = host.Directives.OfType<IDirectiveWithRoot>().FirstOrDefault();
            if (pathedDirective == null)
            {
                log.MarkFailure(COULD_NOT_FIND_DIRECTIVE_PATH);
                return;
            }

            var file = pathedDirective.ApplicationRootDirectory().AppendPath(directive.File).ToFullPath();

            var description = "Resolving the connection string data at " + file;

            log.Trace(description);
            _resolver.Resolve(file);
        }

        public string GetDescription(ResolveDbConnection directive)
        {
            return "Resolving the connection string at " + directive.File;
        }
    }
}