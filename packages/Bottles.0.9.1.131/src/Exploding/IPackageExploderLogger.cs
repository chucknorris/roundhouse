using System.Collections.Generic;

namespace Bottles.Exploding
{
    public interface IPackageExploderLogger
    {
        void WritePackageDirectoryDeleted(string directoryName);
        void WritePackageZipFileExploded(string zipFile, string directoryName);
        void WritePackageZipFileWasSameVersionAsExploded(string file);
        void WritePackageZipsFound(string applicationDirectory, IEnumerable<string> packageFileNames);
        void WriteExistingDirectories(string applicationDirectory, IEnumerable<string> existingDirectories);
    }
}