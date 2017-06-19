using System.IO;
using NUnit.Framework;

namespace roundhouse.tests.infrastructure.filesystem
{
    using roundhouse.infrastructure.filesystem;

    public class DotNetFileSystemAccessSpecs
    {
        public abstract class concern_for_file_system : TinySpec<DotNetFileSystemAccess>
        {
            protected object result;

            public override void Context() { }
            protected override DotNetFileSystemAccess sut => new DotNetFileSystemAccess();
        }

        [Concern(typeof(DotNetFileSystemAccess))]
        public class when_reading_files_with_different_formats : concern_for_file_system
        {
            protected static string utf8_file;
            protected static string ansi_file;

            public override void Because()
            {
                utf8_file = sut.read_file_text(Path.Combine(TestContext.CurrentContext.TestDirectory, @"infrastructure\filesystem\utf8encoded.txt"));
                ansi_file = sut.read_file_text(Path.Combine(TestContext.CurrentContext.TestDirectory, @"infrastructure\filesystem\ansiencoded.txt"));
            }

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