using System.Collections.Generic;
using System.IO;
using FubuCore;
using FubuCore.Util;

namespace Bottles.Zipping
{
    public class ZipFolderRequest
    {
        public FileSet FileSet { get; set; }
        public string RootDirectory { get; set; }
        public string ZipDirectory { get; set; }

        public void WriteToZipFile(IZipFile zipFile)
        {
            if (FileSet == null) return;

            var cache = new Cache<string, string>(file => Path.Combine(ZipDirectory, file.PathRelativeTo(RootDirectory)));

            FileSet.IncludedFilesFor(RootDirectory).Each(cache.FillDefault);
            FileSet.ExcludedFilesFor(RootDirectory).Each(cache.Remove);

            cache.Each((file, name) => zipFile.AddFile(file, Path.GetDirectoryName(name)));
        }

        public bool Equals(ZipFolderRequest other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.FileSet, FileSet) && Equals(other.RootDirectory, RootDirectory) &&
                   Equals(other.ZipDirectory, ZipDirectory);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ZipFolderRequest)) return false;
            return Equals((ZipFolderRequest) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = (FileSet != null ? FileSet.GetHashCode() : 0);
                result = (result*397) ^ (RootDirectory != null ? RootDirectory.GetHashCode() : 0);
                result = (result*397) ^ (ZipDirectory != null ? ZipDirectory.GetHashCode() : 0);
                return result;
            }
        }

        public override string ToString()
        {
            return string.Format("FileSet: {0}, RootDirectory: {1}, ZipDirectory: {2}", FileSet, RootDirectory,
                                 ZipDirectory);
        }
    }
}