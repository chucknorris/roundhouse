using Bottles.Deployment.Commands;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Deployment.Deployers.Installers
{
    public class InstallersDirective : IDirective
    {
        private readonly InstallInput _input = new InstallInput();

        public string AppDirectory
        {
            get { return _input.AppFolder; }
            set { _input.AppFolder = value; }
        }

        public InstallMode Mode
        {
            get
            {
                return _input.ModeFlag;
            }
            set { _input.ModeFlag = value; }
        }

        public string LogFile
        {
            get
            {
                return _input.LogFileFlag;
            }
            set
            {
                _input.LogFileFlag = value;
            }
        }

        public string ConfigFile
        {
            get
            {
                return _input.ConfigFileFlag;
            }
            set
            {
                _input.ConfigFileFlag = value;
            }
        }

        public InstallInput Input
        {
            get { return _input; }
        }

        public string EnvironmentClassName
        {
            get
            {
                return _input.EnvironmentClassNameFlag;
            }
            set
            {
                _input.EnvironmentClassNameFlag = value == string.Empty ? null : value;
            }
        }

        public string EnvironmentAssembly
        {
            get
            {
                return _input.EnvironmentAssemblyFlag;
            }
            set
            {
                _input.EnvironmentAssemblyFlag = value == string.Empty ? null : value;
            }
        }

        public override string ToString()
        {
            return "Running Installers Mode:'{0}'".ToFormat(Mode);
        }

    }

    public class InstallersDeployer : IFinalizer<InstallersDirective>
    {
        public void Execute(InstallersDirective directive, HostManifest host, IPackageLog log)
        {
            log.Trace(directive.Input.Title());
            log.Trace("Writing the installer log file to " + directive.Input.LogFileFlag.ToFullPath());
            new InstallCommand().Execute(directive.Input);
        }

        public string GetDescription(InstallersDirective directive)
        {
            return "Running the installers at " + directive.Input.AppFolder;
        }
    }
}