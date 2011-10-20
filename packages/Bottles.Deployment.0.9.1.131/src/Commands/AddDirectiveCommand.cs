using System;
using System.ComponentModel;
using Bottles.Deployment.Bootstrapping;
using Bottles.Deployment.Runtime;
using Bottles.Deployment.Writing;
using FubuCore;
using FubuCore.CommandLine;
using FubuCore.Reflection;

namespace Bottles.Deployment.Commands
{

    public class AddDirectiveInput
    {
        private readonly Lazy<DeploymentSettings> _settings;
        
        [Description("The recipe to add the directive to.")]
        public string Recipe { get; set; }

        [Description("The host in the recipe to add the directive to.")]
        public string Host { get; set; }

        [Description("The directive to add.")]
        public string Directive { get; set; }

        [Description("The directory where ")]
        public string DeploymentFlag { get; set; }

        [Description("Open the directive file when done.")]
        public bool OpenFlag { get; set; }

        public AddDirectiveInput()
        {
            _settings = new Lazy<DeploymentSettings>(() => DeploymentSettings.ForDirectory(DeploymentFlag));
        }
        
        public DeploymentSettings Settings
        {
            get { return _settings.Value; }
        }

        public string GetHostFileName()
        {
            return Settings.GetHost(Recipe, Host);
        }
    }

    [CommandDescription("Adds a directive to an existing /deployment/recipe/host ", Name="add-directive")]
    public class AddDirectiveCommand : FubuCommand<AddDirectiveInput>
    {
        private readonly IFileSystem _fileSystem;

        public AddDirectiveCommand() : this(new FileSystem())
        {
        }

        public AddDirectiveCommand(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public override bool Execute(AddDirectiveInput input)
        {
            var settings = input.Settings;
            
            var container = DeploymentBootstrapper.Bootstrap(settings);
            var directiveTypeRegistry = container.GetInstance<IDirectiveTypeRegistry>();
            
            return Initialize(directiveTypeRegistry, input);
        }

        public bool Initialize(IDirectiveTypeRegistry registry, AddDirectiveInput input)
        {
            var directiveType = registry.DirectiveTypeFor(input.Directive);
            var directive = directiveType.Create<IDirective>();

            var hostFile = input.GetHostFileName();
            _fileSystem.WriteToFlatFile(hostFile, file =>
            {
                var writer = new DirectiveWriter(file, new TypeDescriptorCache());
                writer.Write(directive);
            });

            if(input.OpenFlag)
            {
                _fileSystem.LaunchEditor(hostFile);
            }

            return true;
        }


    }
}