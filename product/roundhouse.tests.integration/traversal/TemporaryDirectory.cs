using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace roundhouse.tests.integration.traversal
{
    public class TemporaryDirectory : IDisposable
    {
        public class Configuration
        {
            public Configuration(TemporaryDirectory owner)
            {
                directory = owner.directory;
            }

            private DirectoryInfo directory { get; set; }
            public TemporaryDirectory has_subdirectory_named(string directory_name)
            {
                return new TemporaryDirectory(directory, directory_name);
            }

            public TemporaryFile has_file_named(string file_name)
            {
                return new TemporaryFile(directory, file_name);
            }

            public IEnumerable<TemporaryFile> has_files_named(params string[] file_names)
            {
                return file_names.Select(file_name => new TemporaryFile(directory, file_name));
            }
        }
        private readonly DirectoryInfo root;
        public TemporaryDirectory()
        {
            root = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            root.Create();
            directory = root;
        }

        public TemporaryDirectory(DirectoryInfo parent, string directory_name)
        {
            directory = parent.CreateSubdirectory(directory_name);
        }

        public TemporaryDirectory with(Action<Configuration> action)
        {
            action(new Configuration(this));
            return this;
        }

        public DirectoryInfo directory { get; private set; }
    
        public void  Dispose()
        {
            if(root != null && root.Exists)
                root.Delete();
        }
    }

    public class TemporaryFile
    {
        private static readonly Random Random = new Random();
        public TemporaryFile(DirectoryInfo location, string file_name)
        {
            file = new FileInfo(Path.Combine(location.FullName, file_name));
        }
        public void with_content(string content)
        {
            using (FileStream stream = file.OpenWrite())
            {
                byte[] output = Encoding.Default.GetBytes(content);
                stream.Write(output, 0, output.Length);
            }
        }

        public void with_random_content()
        {
            with_content(get_random_string());
        }
        private static string get_random_string()
        {
            const string LOREM_IPSUM = @"
Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque sagittis dapibus quam et luctus. 
Fusce euismod odio id neque condimentum sit amet placerat nisl accumsan. Donec ullamcorper, arcu 
vel tempor convallis, felis erat lacinia nisi, non scelerisque diam tortor eu diam. Morbi dignissim 
iaculis luctus. Morbi vitae lacus orci. Quisque porttitor varius arcu sed aliquam. Morbi ultricies 
leo risus, a vestibulum libero. Etiam ut lectus sed lorem egestas pretium nec et ipsum. Vivamus at
erat nibh, pellentesque facilisis quam. Maecenas diam libero, adipiscing nec vestibulum congue, 
convallis vel magna. Vivamus et enim et nunc scelerisque ultrices vitae at lectus. Quisque commodo 
tincidunt felis eget euismod. Aliquam convallis aliquet est sed vulputate. Proin ornare venenatis sagittis.
";
            return LOREM_IPSUM.Substring(0, Random.Next(20, LOREM_IPSUM.Length));

        }
        public FileInfo file { get; private set; }
    }

    public static class TemporaryFileExtensions
    {
        public static IEnumerable<TemporaryFile> with_content(this IEnumerable<TemporaryFile> files, string content)
        {
            foreach (TemporaryFile file in files)
                file.with_content(content);
            return files;
        }

        public static IEnumerable<TemporaryFile> with_random_content(this IEnumerable<TemporaryFile> files)
        {
            foreach (TemporaryFile file in files)
                file.with_random_content();
            return files;
        }

    }
}