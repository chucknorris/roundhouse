using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;

namespace FubuCore
{
    public enum CopyBehavior
    {
        overwrite,
        preserve
    }

    public class FileSystem : IFileSystem
    {
        public const int BufferSize = 32768;

        public void CreateDirectory(string path)
        {
            if (path.IsEmpty()) return;

            var dir = new DirectoryInfo(path);
            if (dir.Exists)
            {
                return;
            }

            dir.Create();
        }

        public long FileSizeOf(string path)
        {
            return new FileInfo(path).Length;
        }

        public void Copy(string source, string destination)
        {
            Copy(source, destination, CopyBehavior.overwrite);
        }

        public void Copy(string source, string destination, CopyBehavior behavior)
        {
            if (IsFile(source))
            {
                internalFileCopy(source, destination, behavior);
            }
            else
            {
                internalDirectoryCopy(source, destination, behavior);
            }
        }

        public bool IsFile(string path)
        {
            //resolve the path
            path = Path.GetFullPath(path);

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                throw new IOException("This path '{0}' doesn't exist!".ToFormat(path));
            }

            var attr = File.GetAttributes(path);

            return (attr & FileAttributes.Directory) != FileAttributes.Directory;
        }

        public bool FileExists(string filename)
        {
            return File.Exists(filename);
        }

        public void WriteStreamToFile(string filename, Stream stream)
        {
            CreateDirectory(Path.GetDirectoryName(filename));

            var fileSize = 0;
            using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                int bytesRead;
                var buffer = new byte[BufferSize];
                do
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    fileSize += bytesRead;

                    if (bytesRead > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);
                    }
                } while (bytesRead > 0);
                fileStream.Flush();
            }
        }

        public void WriteStringToFile(string filename, string text)
        {
            CreateDirectory(Path.GetDirectoryName(filename));

            File.WriteAllText(filename, text);
        }

        public void AppendStringToFile(string filename, string text)
        {
            File.AppendAllText(filename, text);
        }


        public string ReadStringFromFile(string filename)
        {
            return File.ReadAllText(filename);
        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public void AlterFlatFile(string path, Action<List<string>> alteration)
        {
            var list = new List<string>();

            if (FileExists(path))
            {
                ReadTextFile(path, list.Add);
            }

            list.RemoveAll(x => x.Trim() == string.Empty);

            alteration(list);

            using (var writer = new StreamWriter(path))
            {
                list.Each(x => writer.WriteLine(x));
            }
        }

        public void DeleteDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        public void CleanDirectory(string directory)
        {
            if (directory.IsEmpty()) return;


            DeleteDirectory(directory);
            Thread.Sleep(10);

            CreateDirectory(directory);
        }

        public bool DirectoryExists(string directory)
        {
            return Directory.Exists(directory);
        }

        public void WriteObjectToFile(string filename, object target)
        {
            Debug.WriteLine("Saving to " + filename);
            var serializer = new XmlSerializer(target.GetType());

            CreateDirectory(GetDirectory(filename));

            using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(stream, target);
            }
        }

        public T LoadFromFile<T>(string filename) where T : new()
        {
            if (!FileExists(filename)) return new T();

            var serializer = new XmlSerializer(typeof (T));

            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    return (T) serializer.Deserialize(stream);
                }
                catch (Exception e)
                {
                    var message = "Unable to deserialize the contents of file {0} into an instance of type {1}"
                        .ToFormat(filename, typeof (T).FullName);
                    throw new ApplicationException(message, e);
                }
            }
        }

        public void LaunchEditor(string filename)
        {
            Process.Start("notepad", filename);
        }

        public void DeleteFile(string filename)
        {
            if (!File.Exists(filename)) return;

            File.Delete(filename);
        }

        public void MoveFile(string from, string to)
        {
            CreateDirectory(Path.GetDirectoryName(to));

            try
            {
                File.Move(from, to);
            }
            catch (IOException ex)
            {
                var msg = "Trying to move '{0}' to '{1}'".ToFormat(from, to);
                throw new Exception(msg, ex);
            }
        }

        public void MoveFiles(string from, string to)
        {
            var files = Directory.GetFiles(from, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var partialPath = file.Replace(from, "");
                if (partialPath.StartsWith(@"\")) partialPath = partialPath.Substring(1);
                var newPath = Combine(to, partialPath);
                MoveFile(file, newPath);
            }
        }

        public void MoveDirectory(string from, string to)
        {
            Directory.Move(from, to);
        }

        public IEnumerable<string> ChildDirectoriesFor(string directory)
        {
            if (Directory.Exists(directory))
            {
                return Directory.GetDirectories(directory);
            }

            return new string[0];
        }

        // Only here for mocking/stubbing file system junk
        public IEnumerable<string> FindFiles(string directory, FileSet searchSpecification)
        {
            return searchSpecification.IncludedFilesFor(directory);
        }

        public void ReadTextFile(string path, Action<string> callback)
        {
            if (!FileExists(path)) return;

            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    callback(line.Trim());
                }
            }
        }

        public string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        public string GetDirectory(string path)
        {
            return Path.GetDirectoryName(path);
        }

        private void internalFileCopy(string source, string destination, CopyBehavior behavior)
        {
            var fileName = Path.GetFileName(source);

            var fullSourcePath = Path.GetFullPath(source);
            var fullDestPath = Path.GetFullPath(destination);


            var isFile = destinationIsFile(destination);

            var destinationDir = fullDestPath;
            if (isFile)
            {
                destinationDir = Path.GetDirectoryName(fullDestPath);
            }

            CreateDirectory(destinationDir);

            if (!isFile) //aka its a directory
            {
                fullDestPath = Combine(fullDestPath, fileName);
            }

            var overwrite = behavior == CopyBehavior.overwrite;
            if (!overwrite && FileExists(fullDestPath)) return;

            try
            {
                File.Copy(fullSourcePath, fullDestPath, overwrite);
            }
            catch (Exception ex)
            {
                var msg = "Was trying to copy '{0}' to '{1}' and encountered an error. :(".ToFormat(fullSourcePath,
                                                                                                    fullDestPath);
                throw new Exception(msg, ex);
            }
        }

        private void internalDirectoryCopy(string source, string destination, CopyBehavior behavior)
        {
            var files = Directory.GetFiles(source, "*.*", SearchOption.AllDirectories);
            files.Each(file =>
            {
                var relative = file.PathRelativeTo(source);
                internalFileCopy(file, destination.AppendPath(relative), behavior);
            });
        }

        private bool destinationIsFile(string destination)
        {
            if (FileExists(destination) || DirectoryExists(destination))
            {
                //it exists 
                return IsFile(destination);
            }

            if (destination.Last() == Path.DirectorySeparatorChar)
            {
                //last char is a '/' so its a directory
                return false;
            }

            //last char is not a '/' so its a file
            return true;
        }

        public static string Combine(params string[] paths)
        {
            return paths.Aggregate(Path.Combine);
        }

        public void LaunchBrowser(string filename)
        {
            Process.Start("explorer", filename);
        }

        public static IEnumerable<string> GetChildDirectories(string directory)
        {
            if (!Directory.Exists(directory))
                return new string[0];


            return Directory.GetDirectories(directory);
        }
    }
}