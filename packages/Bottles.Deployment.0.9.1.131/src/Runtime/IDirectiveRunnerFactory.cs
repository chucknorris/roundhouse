using System.Collections.Generic;
using Bottles.Deployment.Parsing;

namespace Bottles.Deployment.Runtime
{
    public interface IDirectiveRunnerFactory
    {
        IEnumerable<IDirectiveRunner> BuildRunners(DeploymentPlan plan);
    }
}