namespace roundhouse.migrators
{
    using System.Collections.Generic;
    using System.Linq;
    using cryptography;
    using infrastructure.logging;

    public sealed class DefaultHashGenerator : HashGenerator
    {
        private const string WindowsLineEnding = "\r\n";
        private const string UnixLineEnding = "\n";
        private const string MacLineEnding = "\r";

        private readonly CryptographicService crypto_provider;
        private readonly IReadOnlyCollection<string> line_ending_variations;

        public DefaultHashGenerator(CryptographicService crypto_provider)
        {
            this.crypto_provider = crypto_provider;
            this.line_ending_variations = new List<string> { WindowsLineEnding, UnixLineEnding, MacLineEnding };
        }

        public string create_hash(string sql_to_run, bool normalizeEndings)
        {
            var input = sql_to_run.Replace(@"'", @"''");
            if (normalizeEndings)
                input = input.Replace(WindowsLineEnding, UnixLineEnding).Replace(MacLineEnding, UnixLineEnding);
            return crypto_provider.hash(input);
        }

        public bool have_same_hash(string script_name, string sql_to_run, string old_text_hash)
        {
            // These check hashes from before the normalization change and after
            // The change does result in a different hash that will not be the result of
            // any sore of file change and therefore should not be logged.
            bool hash_is_same =
                hashes_are_equal(create_hash(sql_to_run, true), old_text_hash) ||   // New hash
                hashes_are_equal(create_hash(sql_to_run, false), old_text_hash);    // Old hash

            if (!hash_is_same)
            {
                // extra checks if only line endings have changed
                hash_is_same = have_same_hash_ignoring_platform(sql_to_run, old_text_hash);
                if (hash_is_same)
                {
                    Log.bound_to(this).log_a_warning_event_containing("Script {0} had different line endings than before but equal content", script_name);
                }
            }

            return hash_is_same;
        }

        private bool hashes_are_equal(string new_text_hash, string old_text_hash)
        {
            return string.Compare(old_text_hash, new_text_hash, true) == 0;
        }

        private bool have_same_hash_ignoring_platform(string sql_to_run, string old_text_hash)
        {
            return line_ending_variations.Any(variation => {
                var normalized_sql = line_ending_variations.Aggregate(sql_to_run, (norm, ending) => norm.Replace(ending, variation));
                return hashes_are_equal(create_hash(normalized_sql, false), old_text_hash);
            });
        }
    }
}