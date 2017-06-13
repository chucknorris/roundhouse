using System.IO;
using FluentAssertions;
using Xunit;

namespace roundhouse.tests.infrastructure.filesystem
{
    using roundhouse.infrastructure.filesystem;

    public class DotNetFileSystemAccessSpecs
    {
        public abstract class concern_for_file_system 
        {
            protected object result;

            protected FileSystemAccess sut;

            protected concern_for_file_system()
            {
                sut = new DotNetFileSystemAccess();
            }
        }

        public class when_reading_files_with_different_formats : concern_for_file_system
        {
            protected string utf8_file;
            protected string ansi_file;

            public when_reading_files_with_different_formats()
            {
                if (File.Exists(@".\infrastructure\filesystem\utf8encoded.txt"))
                {
                    utf8_file = sut.read_file_text(@".\infrastructure\filesystem\utf8encoded.txt");
                }
                else
                {
                    utf8_file = sut.read_file_text(@".\\build_output\RoundhousE\infrastructure\filesystem\utf8encoded.txt");
                }

                if (File.Exists(@".\infrastructure\filesystem\ansiencoded.txt"))
                {
                    ansi_file = sut.read_file_text(@".\infrastructure\filesystem\ansiencoded.txt");
                }
                else
                {
                    ansi_file = sut.read_file_text(@".\build_output\RoundhousE\infrastructure\filesystem\ansiencoded.txt");
                }
            }

            [Fact]
            public void utf8_encoded_file_should_read_correctly()
            {
                utf8_file.Should().Be("INSERT INTO [dbo].[timmy]([value]) VALUES('Gã')");
            }

            [Fact]
            public void ansi_encoded_file_should_read_correctly()
            {
                ansi_file.Should().Be("INSERT INTO [dbo].[timmy]([value]) VALUES('Gã')");
            }
        }
    }
}