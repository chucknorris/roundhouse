using System;
using System.IO;
using Bottles.Exploding;

namespace Bottles.Zipping
{
    public interface IZipFileService
    {
        void CreateZipFile(string fileName, Action<IZipFile> configure);
        void ExtractTo(string fileName, string directory, ExplodeOptions options);
        string GetVersion(string fileName);
        void ExtractTo(string description, Stream stream, string directory);
        PackageManifest GetPackageManifest(string fileName);
    }
}