using System.ComponentModel;
using Bottles.Deployment.Bootstrapping;
using Bottles.Deployment.Diagnostics;
using Bottles.Deployment.Parsing;
using Bottles.Deployment.Runtime;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{

    public class PreviewInput
    {
        public PreviewInput()
        {
            FileFlag = "report.htm";
            Profile = "default";
        }

        [RequiredUsage("named")]
        [Description("The profile to execute")]
        public string Profile { get; set; }

        [Description("Path to where the deployment folder is ~/deployment")]
        public string DeploymentFlag { get; set; }

        [Description("File to write the report to.  Default is 'report.htm'")]
        public string FileFlag { get; set; }


        [Description("Open the report in the default browser")]
        public bool OpenFlag { get; set; }
    }

    [Usage("default", "Creates a preview of the 'default' profile")]
    [Usage("named", "Creates a preview of the named profile from the argument")]
    [CommandDescription("Generates a preview of the deployment")]
    public class PreviewCommand : FubuCommand<PreviewInput>
    {
        public override bool Execute(PreviewInput input)
        {
            var settings = DeploymentSettings.ForDirectory(input.DeploymentFlag);
            var options = new DeploymentOptions(input.Profile){
                ReportName = input.FileFlag
            };

            DeploymentBootstrapper.UsingService<IProfileReader>(settings, x =>
            {
                var plan = x.Read(options);
                var report = new DeploymentReport("Deployment Preview");
                report.WriteDeploymentPlan(plan);

                ConsoleWriter.Write("Writing deployment plan preview to {0}", input.FileFlag.ToFullPath());
                report.Document.WriteToFile(input.FileFlag);
            });

            if(input.OpenFlag)
            {
                new FileSystem().LaunchBrowser(options.ReportName);
            }

            return true;
        }
    }

}