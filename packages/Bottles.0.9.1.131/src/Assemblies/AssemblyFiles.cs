using System.Collections.Generic;

namespace Bottles.Assemblies
{
    public class AssemblyFiles
    {
        public IEnumerable<string> Files { get; set; }
        public IEnumerable<string> PdbFiles { get; set; }

        public IEnumerable<string> MissingAssemblies { get; set; }

        public bool Success { get; set; }

        public static AssemblyFiles Empty
        {
            get
            {
                return new AssemblyFiles
                {
                    Success = true, 
                    Files = new string[0],
                    PdbFiles = new string[0],
                    MissingAssemblies = new string[0]
                };
            }
        }
    }
}