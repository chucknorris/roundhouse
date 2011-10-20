using System.IO;
using System.Linq;
using FubuCore;

namespace Bottles
{
    public static class BottleFiles
    {
        static BottleFiles()
        {
            ContentFolder = "content";
            PackagesFolder = "packages";
        }

        public static readonly string Extension = "zip";

        public static readonly string WebContentFolder = "WebContent";
        public static readonly string VersionFile = ".version";
        public static readonly string DataFolder = "Data";
        public static readonly string ConfigFolder = "Config";
        public static readonly string BinaryFolder = "bin";

        public static string ContentFolder { get; set; }
        public static string PackagesFolder { get; set; }



        public static string FolderForPackage(string name)
        {
            return Path.GetFileNameWithoutExtension(name);
        }

        public static bool IsEmbeddedPackageZipFile(string resourceName)
        {
            var parts = resourceName.Split('.');

            if (parts.Length < 2) return false;
            if (parts.Last().ToLower() != "zip") return false;
            return parts[parts.Length - 2].ToLower().StartsWith("pak");
        }

        public static string EmbeddedPackageFolderName(string resourceName)
        {
            var parts = resourceName.Split('.');
            return parts[parts.Length - 2].Replace("pak-", "");
        }

        public static string DirectoryForPackageZipFile(string applicationDirectory, string file)
        {
            var packageName = Path.GetFileNameWithoutExtension(file);
            return GetDirectoryForExplodedPackage(applicationDirectory, packageName);
        }

        public static string GetDirectoryForExplodedPackage(string applicationDirectory, string packageName)
        {
            var dir = FileSystem.Combine(applicationDirectory, ContentFolder, packageName);
            return dir;
        }

        public static string GetApplicationPackagesDirectory(string applicationDirectory)
        {
            var dir = FileSystem.Combine(applicationDirectory, PackagesFolder);
            return dir;
        }

        public static string GetExplodedPackagesDirectory(string applicationDirectory)
        {
            return FileSystem.Combine(applicationDirectory, ContentFolder);
        }
    }
}