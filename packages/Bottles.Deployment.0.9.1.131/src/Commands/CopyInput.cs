using System.ComponentModel;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{
    public class CopyInput
    {
        [Description("The directory name where the deployment artifacts are going to be written")]
        public string Destination { get; set; }

        [FlagAlias("create-bottles")]
        public bool CreateBottlesFlag { get; set; }

        [Description("Path to where the deployment folder is ~/deployment")]
        public string DeploymentFlag { get; set; }
    }
}