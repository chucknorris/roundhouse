using System;
using System.ComponentModel;
using System.Threading;
using Bottles.Commands;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{
    public class InitializeInput
    {
        private readonly Lazy<DeploymentSettings> _settings;

        public InitializeInput()
        {
            _settings = new Lazy<DeploymentSettings>(() => DeploymentSettings.ForDirectory(DeploymentFlag));
        }

        public InitializeInput(DeploymentSettings settings)
        {
            _settings = new Lazy<DeploymentSettings>(() => settings);
        }

        [Description("Physical folder (or valid alias) of the application")]
        public string DeploymentFlag { get; set; }

        [FlagAlias("f")]
        public bool ForceFlag { get; set; }

        public DeploymentSettings Settings
        {
            get { return _settings.Value; }
        }
    }

    [CommandDescription("Seeds the /deployment folder structure underneath the root directory of a codebase", Name = "init")]
    public class InitializeCommand : FubuCommand<InitializeInput>
    {
        public static readonly string DIRECTORY_ALREADY_EXISTS =
            "Directory {0} already exists. Use the -f flag to overwrite the existing structure";

        public static readonly string DELETING_EXISTING_DIRECTORY = "Deleting existing deployment directory at {0}";

        public override bool Execute(InitializeInput input)
        {
            if (input.DeploymentFlag != null) input.DeploymentFlag = AliasCommand.AliasFolder(input.DeploymentFlag);

            return Initialize(input, new FileSystem(), new SimpleLogger());
        }

        public bool Initialize(InitializeInput input, IFileSystem fileSystem, ISimpleLogger logger)
        {
            var deploymentDirectory = input.Settings.DeploymentDirectory;
            logger.Log("Trying to initialize Bottles deployment folders at {0}", deploymentDirectory);

            if (fileSystem.DirectoryExists(deploymentDirectory))
            {
                if (input.ForceFlag)
                {
                    logger.Log(DELETING_EXISTING_DIRECTORY, deploymentDirectory);
                    fileSystem.CleanDirectory(deploymentDirectory);
                    fileSystem.DeleteDirectory(deploymentDirectory);
                    Thread.Sleep(10); //file system is async
                }
                else
                {
                    logger.Log(DIRECTORY_ALREADY_EXISTS, deploymentDirectory);
                    return false;
                }
            }

            createDirectory(fileSystem, logger, deploymentDirectory);

            createDirectory(fileSystem, logger, input.Settings.BottlesDirectory);
            createDirectory(fileSystem, logger, input.Settings.RecipesDirectory);
            createDirectory(fileSystem, logger, input.Settings.ProfilesDirectory);
            createDirectory(fileSystem, logger, input.Settings.DeployersDirectory);

            return true;
        }

        private static void createDirectory(IFileSystem system, ISimpleLogger logger, params string[] pathParts)
        {
            var directory = FileSystem.Combine(pathParts);

            logger.Log("Creating directory " + directory);
            system.CreateDirectory(directory);
        }
    }
}