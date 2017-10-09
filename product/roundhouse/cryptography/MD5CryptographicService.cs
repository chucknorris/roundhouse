namespace roundhouse.cryptography
{
    using System;
    using System.Text;

    public sealed partial class MD5CryptographicService : CryptographicService
    {
        private readonly LocalMD5Implementation crypto_provider;

        public MD5CryptographicService()
        {
            crypto_provider = new LocalMD5Implementation();
        }

        public string hash(string clear_text_of_what_to_hash)
        {
            const string line_ending_windows = "\r\n";
            const string line_ending_unix = "\n";
            const string line_ending_mac = "\r";
            string normalized_text_of_what_to_hash = clear_text_of_what_to_hash
                .Replace(line_ending_windows, line_ending_unix).Replace(line_ending_mac, line_ending_unix);

            byte[] clear_text_bytes = Encoding.UTF8.GetBytes(normalized_text_of_what_to_hash);

            byte[] cypher_bytes = crypto_provider.ComputeHash(clear_text_bytes);
            return Convert.ToBase64String(cypher_bytes);
        }
    }
}

