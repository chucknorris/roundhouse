using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Bottles.Exploding;
using FubuCore;

namespace Bottles.Assemblies
{

    public class AssemblyPackageInfo : IPackageInfo
    {
        private readonly Assembly _assembly;
        private readonly PackageFiles _files = new PackageFiles();

        private AssemblyPackageInfo(Assembly assembly)
        {
            _assembly = assembly;
        }

        public static AssemblyPackageInfo CreateFor(Assembly assembly)
        {
            var package = new AssemblyPackageInfo(assembly);
            var exploder = PackageExploder.GetPackageExploder(new FileSystem());
            
            exploder.ExplodeAssembly(PackageRegistry.GetApplicationDirectory(), assembly, package.Files);

            return package;
        }

        // TODO -- remove duplication with AssemblyLoader
        public static AssemblyPackageInfo CreateFor(string fileName)
        {
            var assembly = loadPackageAssemblyFromAppBinPath(fileName);
            return CreateFor(assembly);
        }

        private static Assembly loadPackageAssemblyFromAppBinPath(string file)
        {
            var assemblyName = Path.GetFileNameWithoutExtension(file);
            var appBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath ?? AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            if (!Path.GetDirectoryName(file).EqualsIgnoreCase(appBinPath))
            {
                var destFileName = FileSystem.Combine(appBinPath, Path.GetFileName(file));
                if (shouldUpdateFile(file, destFileName))
                {
                    File.Copy(file, destFileName, true);
                }
            }
            return Assembly.Load(assemblyName);
        }

        private static bool shouldUpdateFile(string source, string destination)
        {
            return !File.Exists(destination) || File.GetLastWriteTimeUtc(source) > File.GetLastWriteTimeUtc(destination);
        }

        public PackageFiles Files
        {
            get { return _files; }
        }

        public string Role { get; set; }

        public string Name
        {
            get { return "Assembly:  " + _assembly.FullName; }
        }

        public void LoadAssemblies(IAssemblyRegistration loader)
        {
            loader.Use(_assembly);
        }

        public void ForFolder(string folderName, Action<string> onFound)
        {
            _files.ForFolder(folderName, onFound);
        }

        public void ForData(string searchPattern, Action<string, Stream> dataCallback)
        {
            _files.ForData(searchPattern, dataCallback);
        }

        public IEnumerable<Dependency> GetDependencies()
        {
            yield break;
        }
    }
}