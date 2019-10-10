using roundhouse.cryptography;
using roundhouse.migrators;
using Should;

namespace roundhouse.tests.migrators
{
    public class when_comparing_hashes
    {
        public abstract class concern_for_hash_generator : TinySpec<DefaultHashGenerator>
        {
            protected const string script_text_with_cr = "line1\rline2";
            protected const string script_text_with_lf = "line1\nline2";
            protected const string script_text_with_crlf = "line1\r\nline2";
            protected const string another_script = "lineA\r\nlineB";
            protected string hash_of_script_text_with_cr;
            protected string hash_of_script_text_with_lf;
            protected string hash_of_script_text_with_crlf;
            protected string hash_of_another_script;

            private CryptographicService crypto_service;
            private DefaultHashGenerator default_hash_generator;

            protected concern_for_hash_generator()
            {
                crypto_service = new MD5CryptographicService();
                default_hash_generator = new DefaultHashGenerator(crypto_service);
            }

            public override void Context()
            {
                hash_of_script_text_with_cr = crypto_service.hash(script_text_with_cr);
                hash_of_script_text_with_lf = crypto_service.hash(script_text_with_lf);
                hash_of_script_text_with_crlf = crypto_service.hash(script_text_with_crlf);
                hash_of_another_script = crypto_service.hash(another_script);
            }

            public override void Because() { }

            protected override DefaultHashGenerator sut
            {
                get { return default_hash_generator; }
                set { default_hash_generator = value; }
            }
        }

        [Concern(typeof(DefaultHashGenerator))]
        public class when_only_line_endings_change : concern_for_hash_generator
        {
            [Observation]
            public void if_old_and_new_files_have_CR_should_return_true()
            {
                sut.have_same_hash("file.sql", script_text_with_cr, hash_of_script_text_with_cr).ShouldBeTrue();
            }

            [Observation]
            public void if_old_and_new_files_have_LF_should_return_true()
            {
                sut.have_same_hash("file.sql", script_text_with_lf, hash_of_script_text_with_lf).ShouldBeTrue();
            }

            [Observation]
            public void if_old_and_new_files_have_CRLF_should_return_true()
            {
                sut.have_same_hash("file.sql", script_text_with_crlf, hash_of_script_text_with_crlf).ShouldBeTrue();
            }

            [Observation]
            public void if_old_file_has_CR_and_new_file_has_LF_should_return_true()
            {
                sut.have_same_hash("file.sql", script_text_with_lf, hash_of_script_text_with_cr).ShouldBeTrue();
            }

            [Observation]
            public void if_old_file_has_CR_and_new_file_has_CRLF_should_return_true()
            {
                sut.have_same_hash("file.sql", script_text_with_crlf, hash_of_script_text_with_cr).ShouldBeTrue();
            }

            [Observation]
            public void if_old_file_has_LF_and_new_file_has_CR_should_return_true()
            {
                sut.have_same_hash("file.sql", script_text_with_cr, hash_of_script_text_with_lf).ShouldBeTrue();
            }

            [Observation]
            public void if_old_file_has_LF_and_new_file_has_CRLF_should_return_true()
            {
                sut.have_same_hash("file.sql", script_text_with_crlf, hash_of_script_text_with_lf).ShouldBeTrue();
            }

            [Observation]
            public void if_old_file_has_CRLF_and_new_file_has_CR_should_return_true()
            {
                sut.have_same_hash("file.sql", script_text_with_cr, hash_of_script_text_with_crlf).ShouldBeTrue();
            }

            [Observation]
            public void if_old_file_has_CRLF_and_new_file_has_LF_should_return_true()
            {
                sut.have_same_hash("file.sql", script_text_with_lf, hash_of_script_text_with_crlf).ShouldBeTrue();
            }
        }

        [Concern(typeof(DefaultHashGenerator))]
        public class when_script_text_changes : concern_for_hash_generator
        {
            [Observation]
            public void if_new_file_has_CR_should_return_false()
            {
                sut.have_same_hash("file.sql", script_text_with_cr, hash_of_another_script).ShouldBeFalse();
            }

            [Observation]
            public void if_new_file_has_LF_should_return_false()
            {
                sut.have_same_hash("file.sql", script_text_with_lf, hash_of_another_script).ShouldBeFalse();
            }

            [Observation]
            public void if_new_file_has_CRLF_should_return_false()
            {
                sut.have_same_hash("file.sql", script_text_with_crlf, hash_of_another_script).ShouldBeFalse();
            }
        }
    }
}
