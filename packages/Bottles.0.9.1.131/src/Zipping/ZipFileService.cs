using System;
using System.IO;
using System.Xml.Serialization;
using Bottles.Diagnostics;
using Bottles.Exploding;
using FubuCore;
using FubuCore.CommandLine;
using Ionic.Zip;
using System.Linq;

namespace Bottles.Zipping
{
    public class ZipFileService : IZipFileService
    {
        private readonly IFileSystem _fileSystem;

        public ZipFileService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void CreateZipFile(string fileName, Action<IZipFile> configure)
        {
            ConsoleWriter.Write("    Starting to write contents to new Zip file at " + fileName);

            _fileSystem.CreateDirectory(Path.GetDirectoryName(fileName));

            using (var zipFile = new ZipFile(fileName))
            {
                configure(new ZipFileWrapper(zipFile));
                zipFile.Save();
            }
        }

        public void ExtractTo(string description, Stream stream, string directory)
        {
            LogWriter.Current.Trace("Writing contents of zip file {0} to {1}", description, directory);

            _fileSystem.DeleteDirectory(directory);
            _fileSystem.CreateDirectory(directory);

            
            string fileName = Path.GetTempFileName();
            _fileSystem.WriteStreamToFile(fileName, stream);

            using (var zipFile = new ZipFile(fileName))
            {
                zipFile.ExtractAll(directory, ExtractExistingFileAction.OverwriteSilently);
            }

            _fileSystem.DeleteFile(fileName);
        }

        public void ExtractTo(string fileName, string directory, ExplodeOptions options)
        {
            LogWriter.Current.Trace("Writing contents of zip file {0} to {1}", fileName, directory);

            if (options == ExplodeOptions.DeleteDestination)
            {
                _fileSystem.DeleteDirectory(directory);
            }

            _fileSystem.CreateDirectory(directory);

            using (var zipFile = new ZipFile(fileName))
            {
                zipFile.ExtractAll(directory, ExtractExistingFileAction.OverwriteSilently);
            }
        }

        public string GetVersion(string fileName)
        {
            using (var zipFile = new ZipFile(fileName))
            {

                var entry = zipFile.Entries.SingleOrDefault(x => x.FileName == BottleFiles.VersionFile);
                if (entry == null) return Guid.Empty.ToString();

                var stream = new MemoryStream();
                entry.Extract(stream);

                stream.Position = 0;
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

        public PackageManifest GetPackageManifest(string fileName)
        {
            using (var zipFile = new ZipFile(fileName))
            {

                var entry = zipFile.Entries.SingleOrDefault(x => x.FileName == PackageManifest.FILE);
                if (entry == null) return null;

                var stream = new MemoryStream();
                entry.Extract(stream);

                stream.Position = 0;

                var serializer = new XmlSerializer(typeof (PackageManifest));
                return (PackageManifest) serializer.Deserialize(stream);
            }
        }
    }
}