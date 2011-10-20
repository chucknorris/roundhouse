using System;
using System.Collections.Generic;
using System.Linq;
using Bottles.Assemblies;
using Bottles.Creation;
using FubuCore.CommandLine;

namespace Bottles.Diagnostics
{
    public class PackageLogger : IPackageLogger
    {
        public void WriteAssembliesNotFound(AssemblyFiles theAssemblyFiles, PackageManifest manifest, CreatePackageInput input, string binFolder)
        {
            ConsoleWriter.Write("Did not locate all designated assemblies at '{0}'", binFolder);
            ConsoleWriter.Write("Looking for these assemblies in the package manifest file:");
            manifest.Assemblies.Each(name => ConsoleWriter.Write("  " + name));
            ConsoleWriter.Write("But only found:");
            if(!theAssemblyFiles.Files.Any()) ConsoleWriter.Write("  Found no files");
            theAssemblyFiles.Files.Each(file => ConsoleWriter.Write("  " + file));

            ConsoleWriter.Write("Missing");
            theAssemblyFiles.MissingAssemblies.Each(file => ConsoleWriter.Write("  " + file));
        
            throw new ApplicationException("Invalid package manifest or missing files");
        }
    }
}