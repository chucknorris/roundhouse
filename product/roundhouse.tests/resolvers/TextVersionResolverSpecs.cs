namespace roundhouse.tests.resolvers
{
    using System;
    using bdddoc.core;
    using developwithpassion.bdd.contexts;
    using developwithpassion.bdd.mbunit;
    using developwithpassion.bdd.mbunit.standard;
    using developwithpassion.bdd.mbunit.standard.observations;
    using roundhouse.infrastructure.filesystem;
    using roundhouse.resolvers;
    using Rhino.Mocks;

    public class TextVersionResolverSpecs
    {
        public abstract class concern_for_textversion_resolver : observations_for_a_sut_with_a_contract<VersionResolver, TextVersionResolver>
        {
            [CLSCompliant(false)]
            protected static VersionResolver the_resolver;
        }

        public abstract class concerns_using_a_fake_filesystem : concern_for_textversion_resolver
        {
            protected static FileSystemAccess the_filesystem;
            protected static string the_versionfile;
            private context c = () =>
            {
                the_filesystem = an<FileSystemAccess>();
                the_versionfile = string.Format(@"{0}.txt", Guid.NewGuid());
                provide_a_basic_sut_constructor_argument(the_filesystem);
                provide_a_basic_sut_constructor_argument(the_versionfile);
            };
        }

        [Concern(typeof(TextVersionResolver))]
        public class when_asking_the_resolver_for_the_version_the_version_text_is_trimmed : concerns_using_a_fake_filesystem
        {
            private const string untrimmed = " 1.3.837.1342 \r\n";
            private const string trimmed = "1.3.837.1342";
            private static string result;
            private context c =
                () =>
                {
                    the_filesystem.Stub(x => x.file_exists(the_versionfile)).Return(true);
                    the_filesystem.Stub(x => x.read_file_text(the_versionfile)).Return(untrimmed);
                    the_filesystem.Stub(x => x.get_full_path(the_versionfile)).Return(the_versionfile);
                };

            private because b = () => { result = sut.resolve_version(); };

            [Observation]
            public void untrimmed_version_from_file_is_trimmed_when_resolved()
            {
                result.should_be_equal_to(trimmed);
            }
        }
    }
} 