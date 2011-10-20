using Bottles.Deployment.Parsing;

namespace Bottles.Deployment.Runtime
{
    public interface IDeploymentController
    {
        void Deploy(DeploymentOptions options);
        DeploymentPlan BuildPlan(DeploymentOptions options);
    }
}