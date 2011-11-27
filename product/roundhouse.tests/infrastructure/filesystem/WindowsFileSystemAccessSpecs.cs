using System.IO;

namespace roundhouse.tests.infrastructure.filesystem
{
    using roundhouse.infrastructure.filesystem;
    using bdddoc.core;
    using developwithpassion.bdd.contexts;
    using developwithpassion.bdd.mbunit;
    using developwithpassion.bdd.mbunit.standard;
    using developwithpassion.bdd.mbunit.standard.observations;

    public class WindowsFileSystemAccessSpecs
    {
        public abstract class concern_for_file_system : observations_for_a_sut_with_a_contract<FileSystemAccess, WindowsFileSystemAccess>
        {
            protected static object result;

            context c = () => { };
        }

        [Concern(typeof(WindowsFileSystemAccess))]
        public class when_reading_files_with_different_formats : concern_for_file_system
        {
            protected static string utf8_file;
            protected static string ansi_file;

            because b = () =>
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

            };

            [Observation]
            public void utf8_encoded_file_should_read_correctly()
            {
                utf8_file.should_be_equal_to("INSERT INTO [dbo].[timmy]([value]) VALUES('Gã')");
            }

            [Observation]
            public void ansi_encoded_file_should_read_correctly()
            {
                ansi_file.should_be_equal_to("INSERT INTO [dbo].[timmy]([value]) VALUES('Gã')");
            }
        }
    }
}