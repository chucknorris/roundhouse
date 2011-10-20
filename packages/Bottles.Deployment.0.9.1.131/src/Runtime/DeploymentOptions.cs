using System.Collections.Generic;
using FubuCore.Util;

namespace Bottles.Deployment.Runtime
{
    public class DeploymentOptions
    {
        private readonly IList<string> _recipeNames = new List<string>();
        private readonly IList<string> _importedFolders = new List<string>();
        private readonly Cache<string, string> _overrides = new Cache<string,string>();

        public DeploymentOptions() : this("default")
        {
        }

        public DeploymentOptions(string profileName)
        {
            ProfileName = profileName;
            ProfileFileName = "C:\\TODO";
            ReportName = "report.htm";
        }

        public string ProfileName { get; set; }
        public string ProfileFileName { get; set; }

        public string ReportName { get; set; }

        public IList<string> RecipeNames
        {
            get { return _recipeNames; }
        }

        public Cache<string, string> Overrides
        {
            get { return _overrides; }
        }

        public IList<string> ImportedFolders
        {
            get { return _importedFolders; }
        }
    }
}