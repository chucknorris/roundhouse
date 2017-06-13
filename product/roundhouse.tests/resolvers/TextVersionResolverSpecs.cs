using FluentAssertions;
using NSubstitute;
using Xunit;

namespace roundhouse.tests.resolvers
{
    using System;
    using roundhouse.infrastructure.filesystem;
    using roundhouse.resolvers;

    public class TextVersionResolverSpecs
    {
        public abstract class concern_for_textversion_resolver 
        {
            //protected VersionResolver the_resolver;
            protected FileSystemAccess the_filesystem;
            protected string the_versionfile;
            protected VersionResolver sut;

            protected concern_for_textversion_resolver()
            {
            }
        }

        public abstract class concerns_using_a_fake_filesystem : concern_for_textversion_resolver
        {
            protected concerns_using_a_fake_filesystem()
            {
                the_filesystem = Substitute.For<FileSystemAccess>();
                the_versionfile = string.Format(@"{0}.txt", Guid.NewGuid());
            }
        }

        public class when_asking_the_resolver_for_the_version_the_version_text_is_trimmed : concerns_using_a_fake_filesystem
        {
            private const string untrimmed = " 1.3.837.1342 \r\n";
            private const string trimmed = "1.3.837.1342";
            private string result;

            public  when_asking_the_resolver_for_the_version_the_version_text_is_trimmed()
                {
                    the_filesystem.file_exists(the_versionfile).Returns(true);
                    the_filesystem.read_file_text(the_versionfile).Returns(untrimmed);
                    the_filesystem.get_full_path(the_versionfile).Returns(the_versionfile);

                    sut = new TextVersionResolver(the_filesystem, the_versionfile);

                    result = sut.resolve_version(); 
                }

            [Fact]
            public void untrimmed_version_from_file_is_trimmed_when_resolved()
            {
                result.Should().Be(trimmed);
            }
        }
    }
} 