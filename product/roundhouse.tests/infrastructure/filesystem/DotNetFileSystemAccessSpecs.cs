using System;
using System.IO;
using NUnit.Framework;

namespace roundhouse.tests.infrastructure.filesystem
{
    using roundhouse.consoles;
    using roundhouse.infrastructure.filesystem;
    using System.Text;

    public class DotNetFileSystemAccessSpecs
    {
        public abstract class concern_for_file_system : TinySpec<DotNetFileSystemAccess>
        {
            protected DotNetFileSystemAccess dot_net_file_system_access;

            protected string utf_expected_string;

            protected string very_small_file;
            protected string utf8_file_with_bom;
            protected string utf8_file_no_bom;
            protected string utf16LE_file_with_bom;
            protected string utf16BE_file_with_bom;
            protected string utf32LE_file_with_bom;
            protected string utf32BE_file_with_bom;
            protected string ansi_file;

            protected override DotNetFileSystemAccess sut
            {
                get { return dot_net_file_system_access; }
                set { dot_net_file_system_access = value; }
            }

            private string read_test_file(string file_name)
            {
                return sut.read_file_text(
                    Path.Combine(
                        TestContext.CurrentContext.TestDirectory,
                        "infrastructure",
                        "filesystem",
                        file_name));
            }

            public override void Because()
            {
                // NOTE: When testing encodings, its important to do it so the result doesnt depend on how the compiler itself will read the source file.
                // Best way to do it is to work with byte representation i.e. do not include any non ASCII characters (> 127) in source code directly...
                string unicode_character = Encoding.UTF8.GetString(new byte[] { 0xc3, 0xa3 });  // small "a" with tilde (utf-8)
                utf_expected_string = string.Format("INSERT INTO [dbo].[timmy]([value]) VALUES('G{0}')", unicode_character);

                very_small_file = read_test_file("very_small_file.txt");
                utf8_file_with_bom = read_test_file("utf8_with_bom.txt");
                utf8_file_no_bom = read_test_file("utf8_no_bom.txt");
                utf16LE_file_with_bom = read_test_file("utf16LE_with_bom.txt");
                utf16BE_file_with_bom = read_test_file("utf16BE_with_bom.txt");
                utf32LE_file_with_bom = read_test_file("utf32LE_with_bom.txt");
                utf32BE_file_with_bom = read_test_file("utf32BE_with_bom.txt");

                ansi_file = read_test_file("ansiencoded.txt");
            }

            [Observation]
            public void very_small_file_should_read_correctly()
            {
                very_small_file.should_be_equal_to("GO");
            }

            [Observation]
            public void utf8_encoded_file_with_bom_should_read_correctly()
            {
                utf8_file_with_bom.should_be_equal_to(utf_expected_string);
            }

            [Observation]
            public void utf16LE_encoded_file_with_bom_should_read_correctly()
            {
                utf16LE_file_with_bom.should_be_equal_to(utf_expected_string);
            }

            [Observation]
            public void utf16BE_encoded_file_with_bom_should_read_correctly()
            {
                utf16BE_file_with_bom.should_be_equal_to(utf_expected_string);
            }

            [Observation]
            public void utf32LE_encoded_file_with_bom_should_read_correctly()
            {
                utf32LE_file_with_bom.should_be_equal_to(utf_expected_string);
            }

            [Observation]
            public void utf32BE_encoded_file_with_bom_should_read_correctly()
            {
                utf32BE_file_with_bom.should_be_equal_to(utf_expected_string);
            }
        }

        
        [Concern(typeof(DotNetFileSystemAccess))]
        public class when_reading_files_with_different_formats_with_default_configuration : concern_for_file_system
        {
            public override void Context()
            {
                dot_net_file_system_access = new DotNetFileSystemAccess(new DefaultConfiguration());
            }

            [Observation]
            public void utf8_encoded_file_without_bom_should_read_correctly()
            {
                utf8_file_no_bom.should_be_equal_to(utf_expected_string);
            }
        }

#if net48
        [Concern(typeof(DotNetFileSystemAccess))]
        public class when_reading_files_with_different_formats_with_ansi_encoding_configuration : concern_for_file_system
        {
            private readonly Encoding ansi_encoding = Encoding.GetEncoding("windows-1252");

            public override void Context()
            {
                dot_net_file_system_access = new DotNetFileSystemAccess(
                    new DefaultConfiguration { DefaultEncoding =  ansi_encoding }
                );
            }

            [Observation]
            public void ansi_encoded_file_shoul_read_correctly()
            {
                // small "a" with tilde in Windows-1252
                string extended_ascii_character = ansi_encoding.GetString(new byte[] { 0xe3 }); 

                ansi_file.should_be_equal_to(string.Format("INSERT INTO [dbo].[timmy]([value]) VALUES('G{0}')", extended_ascii_character));
            }
        }
        
#endif
        
    }
}