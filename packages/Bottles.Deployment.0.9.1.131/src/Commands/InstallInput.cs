using System.ComponentModel;
using System.IO;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{
    public class InstallInput
    {
        public InstallInput()
        {
            ModeFlag = InstallMode.install;
            LogFileFlag = "installation.htm";
            ConfigFileFlag = "web.config";
        }

        [Description("Root folder (or alias) of the fubu application")]
        public string AppFolder { get; set; }

        [Description("Determines what actions are executed for each installer.  'install' is the default")]
        public InstallMode ModeFlag { get; set; }

        [Description("Overrides the location of the log file produced, otherwise 'installation.htm' is the default")]
        public string LogFileFlag { get; set; }

        [Description("When specified, opens the resulting log file in the default web browser")]
        public bool OpenFlag { get; set; }

        [Description("The IEnvironment class to run during an install")]
        [FlagAlias("class")]
        public string EnvironmentClassNameFlag { get; set; }

        /// <summary>
        /// The assembly where the environment class is located
        /// </summary>
        [Description("The assembly containing the IEnvironment class to run during an install")]
        [FlagAlias("assembly")]
        public string EnvironmentAssemblyFlag { get; set; }

        [FlagAlias("config-file")]
        [Description("The name of the .Net AppDomain config file to use while running the installers")]
        public string ConfigFileFlag { get; set; }


        public string ManifestFileName
        {
            get { return Path.GetFullPath(FileSystem.Combine(AppFolder, PackageManifest.FILE)); }
        }

        public string Title()
        {
            var format = "";

            switch (ModeFlag)
            {
                case InstallMode.install:
                    format = "Installing the application at {0}";
                    break;

                case InstallMode.check:
                    format = "Running environment checks for {0}";
                    break;

                case InstallMode.all:
                    format = "Installing and running environment checks for {0}";
                    break;
            }

            return format.ToFormat(ManifestFileName);
        }

        public override string ToString()
        {
            return string.Format("Executing installers at {0} with mode {1}", AppFolder, ModeFlag);
        }
    }
}