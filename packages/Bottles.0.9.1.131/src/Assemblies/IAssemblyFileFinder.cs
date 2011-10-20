using System.Collections.Generic;

namespace Bottles.Assemblies
{
    public interface IAssemblyFileFinder
    {
        AssemblyFiles FindAssemblies(string binDirectory, IEnumerable<string> assemblyNames);
    }
}