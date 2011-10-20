using System;
using System.ComponentModel;
using Bottles.Creation;
using FubuCore;
using FubuCore.CommandLine;
using System.Collections.Generic;
using System.Linq;

namespace Bottles.Commands
{
    public enum AssembliesCommandMode
    {
        add,
        remove,
        list
    }

    public class AssembliesInput
    {
        [Description("Add, remove, or list the assemblies for this manifest")]
        [RequiredUsage("all", "single")]
        public AssembliesCommandMode Mode { get; set; }

        [Description("The package or application directory")]
        [RequiredUsage("all", "single")]
        public string Directory { get; set; }

        [Description("Overrides the name of the manifest file if it's not the default .package-manifest or .fubu-manifest")]
        [FlagAlias("file")]
        public string FileNameFlag { get; set; }


        [RequiredUsage("single")]
        [Description("Add or removes the named assembly")]
        public string AssemblyName { get; set; }

        [Description("Opens the manifest file in your editor")]
        public bool OpenFlag { get; set; }

        [Description("Choose the compilation target for any assemblies.  Default is debug")]
        public CompileTargetEnum TargetFlag { get; set; }

        [IgnoreOnCommandLine]
        public PackageManifest Manifest { get; set;}

        [IgnoreOnCommandLine]
        public string BinariesFolder { get; set;}



        public void FindManifestAndBinaryFolders(IFileSystem fileSystem)
        {
            BinariesFolder = fileSystem.FindBinaryDirectory(Directory, TargetFlag);

            Manifest = fileSystem.TryFindManifest(Directory, FileNameFlag) ??
                       fileSystem.LoadPackageManifestFrom(Directory);
        }

        public void Save(IFileSystem fileSystem)
        {
            fileSystem.PersistToFile(Manifest, Manifest.ManifestFileName);
        }

        public void RemoveAssemblies(IFileSystem fileSystem)
        {
            if (AssemblyName.IsNotEmpty())
            {
                Manifest.RemoveAssembly(AssemblyName);
            }
            else
            {
                Manifest.RemoveAllAssemblies();
            }
            
            Save(fileSystem);
        }

        public void AddAssemblies(IFileSystem fileSystem)
        {
            if (AssemblyName.IsNotEmpty())
            {
                Manifest.AddAssembly(AssemblyName);
            }
            else
            {
                fileSystem.FindAssemblyNames(BinariesFolder).Each(name => Manifest.AddAssembly(name));
            }
            
            Save(fileSystem);
        }
    }

    [Usage("all", "Remove or adds all assemblies to the manifest file")]
    [Usage("single", "Removes or adds a single assembly name to the manifest file")]
    [CommandDescription("Adds assemblies to a given manifest")]
    public class AssembliesCommand : FubuCommand<AssembliesInput>
    {
        public override bool Execute(AssembliesInput input)
        {
            input.Directory = AliasCommand.AliasFolder(input.Directory);

            var fileSystem = new FileSystem();
            input.FindManifestAndBinaryFolders(fileSystem);
            
            

            return Execute(fileSystem, input);
        }

        private bool Execute(IFileSystem fileSystem, AssembliesInput input)
        {
            // return false if manifest does not exist
            if (input.Manifest == null)
            {
                throw new CommandFailureException("Could not find a PackageManifest in the directory " + input.Directory);
            }


            switch (input.Mode)
            {
                case AssembliesCommandMode.add:
                    input.AddAssemblies(fileSystem);
                    break;

                case AssembliesCommandMode.remove:
                    input.RemoveAssemblies(fileSystem);
                    break;

                case AssembliesCommandMode.list:
                    ListAssemblies(fileSystem, input);
                    break;
            }

            if (input.OpenFlag)
            {
                fileSystem.LaunchEditor(input.Manifest.ManifestFileName);
            }

            return true;
        }

        public static void ListAssemblies(IFileSystem fileSystem, AssembliesInput input)
        {
            ConsoleWriter.Write("Assemblies referenced in {0} are:", input.Manifest.ManifestFileName);

            input.Manifest.Assemblies.Each(name => ConsoleWriter.Write(" * " + name));

            ConsoleWriter.Line();
            ConsoleWriter.Write("Assemblies at {0} not referenced in the manifest:");

            fileSystem
                .FindAssemblyNames(input.BinariesFolder)
                .Where(x => !input.Manifest.Assemblies.Contains(x))
                .Each(x => ConsoleWriter.Write(" * " + x));
        }
    }
}