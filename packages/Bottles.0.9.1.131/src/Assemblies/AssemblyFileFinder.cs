using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;

namespace Bottles.Assemblies
{
    public class AssemblyFileFinder : IAssemblyFileFinder
    {
        private readonly IFileSystem _fileSystem;

        public AssemblyFileFinder(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public AssemblyFiles FindAssemblies(string binDirectory, IEnumerable<string> assemblyNames)
        {
            if (!assemblyNames.Any()) return AssemblyFiles.Empty;
            var assemblySet = FileSet.ForAssemblyNames(assemblyNames);
            var debugSet = FileSet.ForAssemblyDebugFiles(assemblyNames);

            var files = new AssemblyFiles(){
                Files = _fileSystem.FindFiles(binDirectory, assemblySet),
                PdbFiles = _fileSystem.FindFiles(binDirectory, debugSet)
            };

            var assembliesFound = files.Files.Select(Path.GetFileNameWithoutExtension).Select(x => x.ToLowerInvariant());
            files.MissingAssemblies = assemblyNames.Select(x => x.ToLowerInvariant()).Where(name => !assembliesFound.Contains(name));
            files.Success = !files.MissingAssemblies.Any();

            return files;
        }
    }
}