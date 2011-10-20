using System.Collections.Generic;
using Bottles.Deployment.Configuration;

namespace Bottles.Deployment.Parsing
{
    public class DeploymentGraph
    {
        public EnvironmentSettings Environment { get; set; }
        public IEnumerable<Recipe> Recipes { get; set; }
        public Profile Profile { get; set; }

        public DeploymentSettings Settings { get; set; }
    }
}