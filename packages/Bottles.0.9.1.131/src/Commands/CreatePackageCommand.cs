using System;
using Bottles.Assemblies;
using Bottles.Creation;
using Bottles.Diagnostics;
using Bottles.Zipping;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Commands
{
    [CommandDescription("Create a package file from a package directory", Name = "create-pak")]
    public class CreatePackageCommand : FubuCommand<CreatePackageInput>
    {
        public override bool Execute(CreatePackageInput input)
        {
            ConsoleWriter.Write("  Creating package at " + input.PackageFolder);

            input.PackageFolder = AliasCommand.AliasFolder(input.PackageFolder);

            Execute(input, new FileSystem());
            return true;
        }

        public void Execute(CreatePackageInput input, IFileSystem fileSystem)
        {
            //TODO: harden
            if (fileSystem.FileExists(input.ZipFile) && !input.ForceFlag)
            {
                WriteZipFileAlreadyExists(input.ZipFile);
                return;
            }

            // Delete the file if it exists?
            if (fileSystem.PackageManifestExists(input.PackageFolder))
            {
                fileSystem.DeleteFile(input.ZipFile);
                CreatePackage(input, fileSystem);
            }
            else
            {
                WritePackageManifestDoesNotExist(input.PackageFolder);
            }
        }

        public virtual void WriteZipFileAlreadyExists(string zipFileName)
        {
            ConsoleWriter.Write("Package Zip file already exists at '{0}'.  Use the -f (force) flag to overwrite the existing flag", zipFileName);
        }

        public virtual void WritePackageManifestDoesNotExist(string packageFolder)
        {
            ConsoleWriter.Write(
                "The requested package folder at '{0}' does not have a package manifest.  Run 'fubu init-pak \"{0}\"' first.",
                packageFolder);
        }

        public virtual void CreatePackage(CreatePackageInput input, IFileSystem fileSystem)
        {
            var fileName = FileSystem.Combine(input.PackageFolder, input.ManifestFileNameFlag ?? PackageManifest.FILE);
            var manifest = fileSystem.LoadFromFile<PackageManifest>(fileName);

            var creator = new PackageCreator(fileSystem, new ZipFileService(fileSystem), new PackageLogger(), new AssemblyFileFinder(new FileSystem()));
            creator.CreatePackage(input, manifest);
        }
    }
}