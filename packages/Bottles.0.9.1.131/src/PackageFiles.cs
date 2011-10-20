using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;
using FubuCore.Util;

namespace Bottles
{
    public interface IPackageFiles
    {
        void RegisterFolder(string folderName, string directory);
        void ForFolder(string folderName, Action<string> onFound);
        void ForData(string searchPattern, Action<string, Stream> dataCallback);
    }

    public class PackageFiles : IPackageFiles
    {
        private readonly Cache<string, string> _directories = new Cache<string, string>();

        /// <summary>
        ///   This a way to abstract the file system. You can add the directory
        /// </summary>
        /// <param name = "folderName">the name of the folder as perceived in the package</param>
        /// <param name = "directory">the actual name of the directory</param>
        public void RegisterFolder(string folderName, string directory)
        {
            if (folderName.Contains(Path.PathSeparator))
            {
                throw new ArgumentException(
                    "The path you have provided '{0}' contains a PathSeparator ('{1}') please do not register anything but root directories."
                        .ToFormat(folderName, Path.PathSeparator));
            }

            _directories[folderName] = directory;
        }

        /// <summary>
        ///   If you register the directory 'C:\bob' as 'bobFolder' for folder will call your
        ///   call back with 'C:\bob' if you ask for folder 'bobFolder'
        /// </summary>
        /// <param name = "folderName"></param>
        /// <param name = "onFound"></param>
        public void ForFolder(string folderName, Action<string> onFound)
        {
            _directories.WithValue(folderName, onFound);
        }

        /// <summary>
        ///   Will call the 'dataCallback' for each file that is found matching the pattern.
        ///   If you were to type '*.jpg' you would get all of the *.jpg in the whole package
        ///   If you were to type 'images/*.jpg' you would get only the .jpg in the 'images'
        ///   directory.
        /// </summary>
        /// <param name = "searchPattern">*.jpg, images/*.jpg</param>
        /// <param name = "dataCallback">Called back with the entries name and data stream</param>
        public void ForData(string searchPattern, Action<string, Stream> dataCallback)
        {
            if (!_directories.Has(BottleFiles.DataFolder))
            {
                return;
            }
            
            var dirParts = searchPattern.Split(Path.DirectorySeparatorChar);

            var folderPath = _directories[BottleFiles.DataFolder].ToFullPath();
            var filePattern = searchPattern;

            if (dirParts.Count() > 1)
            {
                var rootDir = dirParts.Take(dirParts.Length - 1).Join(Path.DirectorySeparatorChar.ToString());
                folderPath = FileSystem.Combine(folderPath, rootDir);

                if (rootDir.IsNotEmpty() && !Directory.Exists(folderPath))
                {
                    return;
                }

                filePattern = dirParts.Last();
            }

            if (!Directory.Exists(folderPath)) return;

            Directory.GetFiles(folderPath, filePattern, SearchOption.AllDirectories).Each(fileName =>
            {
                var name = fileName.PathRelativeTo(folderPath);
                using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    dataCallback(name, stream);
                }
            });
        }
    }
}