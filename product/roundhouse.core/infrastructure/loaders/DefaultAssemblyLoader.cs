namespace roundhouse.infrastructure.loaders
{
    using System.Reflection;

    public class DefaultAssemblyLoader
    {
        public static Assembly load_assembly(string assembly_name)
        {
            return Assembly.Load(assembly_name);
        }
    }
}