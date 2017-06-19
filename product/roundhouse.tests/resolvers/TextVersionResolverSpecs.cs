namespace roundhouse.tests.resolvers
{
    using System;
    using roundhouse.infrastructure.filesystem;
    using roundhouse.resolvers;
    using Moq;

    public class TextVersionResolverSpecs
    {
        public abstract class concern_for_textversion_resolver : TinySpec<TextVersionResolver>
        {
            protected Mock<FileSystemAccess> filesystem_mock;
            protected FileSystemAccess the_filesystem;
            protected string the_versionfile;
            protected override TextVersionResolver sut => new TextVersionResolver(the_filesystem, the_versionfile);

            public concern_for_textversion_resolver()
            {
                filesystem_mock = new Mock<FileSystemAccess>();
                the_filesystem = filesystem_mock.Object;
                the_versionfile = string.Format(@"{0}.txt", Guid.NewGuid());
            }
        }

        public abstract class concerns_using_a_fake_filesystem : concern_for_textversion_resolver
        {

        }

        [Concern(typeof(TextVersionResolver))]
        public class when_asking_the_resolver_for_the_version_the_version_text_is_trimmed : concerns_using_a_fake_filesystem
        {
            private const string untrimmed = " 1.3.837.1342 \r\n";
            private const string trimmed = "1.3.837.1342";
            private static string result;

            public override void Context()
            {
                filesystem_mock.Setup(x => x.file_exists(the_versionfile)).Returns(true);
                filesystem_mock.Setup(x => x.read_file_text(the_versionfile)).Returns(untrimmed);
                filesystem_mock.Setup(x => x.get_full_path(the_versionfile)).Returns(the_versionfile);
            }

            public override void Because() => result = sut.resolve_version();

            [Observation]
            public void untrimmed_version_from_file_is_trimmed_when_resolved()
            {
                result.should_be_equal_to(trimmed);
            }
        }
    }
}