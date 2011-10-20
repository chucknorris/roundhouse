using Bottles.Deployment.Parsing;
using Bottles.Deployment.Runtime;

namespace Bottles.Deployment.Diagnostics
{
    public interface IDiagnosticsReporter
    {
        void WriteReport(DeploymentOptions options, DeploymentPlan plan);
    }
}