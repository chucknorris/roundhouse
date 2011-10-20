using System.ComponentModel;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Commands
{
    public class InitPakInput
    {
        [Description("The physical path to the new package")]
        public string Path { get; set; }

        [Description("The name of the new package")]
        public string Name { get; set; }

        [Description("What role should this pak play - Options: module (default), binaries, config, application")]
        public string RoleFlag { get; set; }

        [Description("Creates a folder alias for the package folder.  Equivalent to fubu alias <folder> <alias>")]
        public string AliasFlag { get; set; }

        [Description("Opens the package manifest file in notepad")]
        public bool OpenFlag { get; set; }

        [Description("There is no web content to include")]
        [FlagAlias("noweb")]
        public bool NoWebContentFlag { get; set; }

        [Description("Force the command to overwrite any existing manifest file if using the -create flag")]
        [FlagAlias("f")]
        public bool ForceFlag { get; set; }

    }

    [CommandDescription("Initialize a package manifest", Name = "init-pak")]
    public class InitPakCommand : FubuCommand<InitPakInput>
    {
        public override bool Execute(InitPakInput input)
        {
            //this will create an alias entry
            new AliasCommand().Execute(new AliasInput{
                Folder = input.Path,
                Name = input.AliasFlag ?? input.Name.ToLower()
            });

            Execute(input, new FileSystem());

            return true;
        }

        public void Execute(InitPakInput input, IFileSystem fileSystem)
        {
            var assemblyName = fileSystem.GetFileName(input.Path);

            var manifest = new PackageManifest{
                Name = input.Name
            };

            manifest.AddAssembly(assemblyName);

            manifest.SetRole(input.RoleFlag ?? BottleRoles.Module);

            if (input.NoWebContentFlag)
            {
                manifest.ContentFileSet = new FileSet {DeepSearch = false, Include="*.config"};
            }

            

            

			if(input.ForceFlag || !fileSystem.FileExists(FileSystem.Combine(input.Path, PackageManifest.FILE)))
			{
				fileSystem.PersistToFile(manifest, input.Path, PackageManifest.FILE);
			}
            

            if (input.OpenFlag)
            {
                fileSystem.LaunchEditor(input.Path, PackageManifest.FILE);
            }
        }
    }
}