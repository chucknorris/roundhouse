using Bottles.Deployment.Configuration;
using FubuCore;

namespace Bottles.Deployment.Deployers.Configuration
{
    public class CentralConfig : IDirective
    {
        public CentralConfig()
        {
            Directory = FileSystem.Combine(EnvironmentSettings.ROOT.ToSubstitution(), "config");
            CopyBehavior = CopyBehavior.overwrite;
            ProfileFile = EnvironmentSettings.ROOT.ToSubstitution().AppendPath("profile", "Profile.config");
            EnvironmentFile = Directory.AppendPath("EnvironmentSettings.config");
        }

        public string Directory { get; set; }
        public CopyBehavior CopyBehavior { get; set; }

        public string ProfileFile { get; set; }
        public string EnvironmentFile { get; set; }

        public override string ToString()
        {
            return "Central Config Dir: '{0}' Behavior: '{1}'".ToFormat(Directory, CopyBehavior);
        }
    }
}