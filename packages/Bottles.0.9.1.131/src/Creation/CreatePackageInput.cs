using System.ComponentModel;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Creation
{
    public enum CompileTargetEnum
    {
        debug,
        release
    }

    public class CreatePackageInput
    {
        public CreatePackageInput()
        {
            TargetFlag = CompileTargetEnum.debug;
        }

        [Description("The root physical folder (or valid alias) of the package")]
        public string PackageFolder { get; set; }

        [Description("The filepath where the zip file for the package will be written ie. ./blue/my-pak.zip")]
        public string ZipFile { get; set; }

        [IgnoreOnCommandLine]
        public string BottlesDirectory { get; set;}

        [Description("Includes any matching .pdb files for the package assemblies")]
        public bool PdbFlag { get; set; }

        [Description("Forces the command to delete any existing zip file first")]
        [FlagAlias("f")]
        public bool ForceFlag { get; set; }

        [Description("Choose the compilation target for any assemblies")]
        public CompileTargetEnum TargetFlag { get; set; }

        [Description("Overrides the name of the manifest file")]
        [FlagAlias("file")]
        public string ManifestFileNameFlag { get; set; }

        public string GetZipFileName(PackageManifest manifest)
        {
            return ZipFile ?? FileSystem.Combine(BottlesDirectory, manifest.Name + ".zip");
        }
    }
}