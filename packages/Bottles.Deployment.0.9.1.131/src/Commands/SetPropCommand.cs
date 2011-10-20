using System.ComponentModel;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{
    public class SetHostPropInput
    {
        [Description("Name of the recipe")]
        public string Recipe { get; set; }

        [Description("Name of the host")]
        public string Host { get; set; }

        [Description("Property=Value declaration of the property to write out")]
        public string PropertyValue { get; set; }

        [Description("Overrides the location of the ~/deployment directory")]
        public string DeploymentFlag { get; set; }
    }

    [CommandDescription("Writes or overwrites a single directive property in a Recipe/Host file", Name = "set-host-prop")]
    public class SetHostPropCommand : FubuCommand<SetHostPropInput>
    {
        public override bool Execute(SetHostPropInput input)
        {
            var path = DeploymentSettings.ForDirectory(input.DeploymentFlag).GetHost(input.Recipe, input.Host);
            new FileSystem().WriteProperty(path, input.PropertyValue);

            return true;
        }
    }

    public class SetEnvPropInput
    {
        [Description("Property=Value declaration of the property to write out")]
        public string PropertyValue { get; set; }

        [Description("Overrides the location of the ~/deployment directory")]
        public string DeploymentFlag { get; set; }
    }

    [CommandDescription("Writes or overwrites a single directive property in the environment.settings file", Name = "set-env-prop")]
    public class SetEnvPropCommand : FubuCommand<SetEnvPropInput>
    {
        public override bool Execute(SetEnvPropInput input)
        {
            var path = DeploymentSettings.ForDirectory(input.DeploymentFlag).EnvironmentFile();
            new FileSystem().WriteProperty(path, input.PropertyValue);

            return true;
        }
    }

    public class SetProfilePropInput
    {
        [Description("Name of the profile")]
        public string Profile { get; set; }
        
        [Description("Property=Value declaration of the property to write out")]
        public string PropertyValue { get; set; }

        [Description("Overrides the location of the ~/deployment directory")]
        public string DeploymentFlag { get; set; }
    }

    [CommandDescription("Writes or overwrites a single directive property in a profile file", Name = "set-profile-prop")]
    public class SetProfilePropCommand : FubuCommand<SetProfilePropInput>
    {

        public override bool Execute(SetProfilePropInput input)
        {
            var path = DeploymentSettings.ForDirectory(input.DeploymentFlag).ProfileFileNameFor(input.Profile);
            new FileSystem().WriteProperty(path, input.PropertyValue);

            return true;
        }
    }

    
}