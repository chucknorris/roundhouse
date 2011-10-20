using System;
using Bottles.Deployment.Runtime;

namespace Bottles.Deployment.Parsing
{
    public interface IProfileReader
    {
        DeploymentPlan Read(DeploymentOptions options);
    }

    public class ProfileReader : IProfileReader
    {
        private readonly IDeploymentGraphReader _reader;

        public ProfileReader(IDeploymentGraphReader reader)
        {
            _reader = reader;
        }

        public DeploymentPlan Read(DeploymentOptions options)
        {
            var graph = _reader.Read(options);
            return new DeploymentPlan(options, graph);
        }
    }
}