using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles.Exploding
{
    public interface IPackageExploder
    {
        IEnumerable<string> ExplodeAllZipsAndReturnPackageDirectories(string applicationDirectory, IPackageLog log);
        void Explode(string sourceZipFile, string destinationDirectory, ExplodeOptions options);
        void CleanAll(string applicationDirectory);
        string ReadVersion(string directoryName);
        IEnumerable<string> ExplodeDirectory(ExplodeDirectory directory);
    }
}