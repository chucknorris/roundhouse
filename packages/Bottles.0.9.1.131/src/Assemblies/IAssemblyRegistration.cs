using System.Reflection;

namespace Bottles.Assemblies
{
    public interface IAssemblyRegistration
    {
        void Use(Assembly assembly);
        void LoadFromFile(string fileName, string assemblyName);
    }
}