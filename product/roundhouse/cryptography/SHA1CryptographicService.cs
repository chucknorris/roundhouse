namespace roundhouse.cryptography
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public sealed class SHA1CryptographicService : CryptographicService
    {
        private readonly SHA1 crypto_provider;

        public SHA1CryptographicService()
        {
            crypto_provider = SHA1.Create();
        }

        public string hash(string clear_text_of_what_to_hash)
        {
            byte[] clear_text_bytes = Encoding.UTF8.GetBytes(clear_text_of_what_to_hash);
            byte[] cypher_bytes = crypto_provider.ComputeHash(clear_text_bytes);
            return Convert.ToBase64String(cypher_bytes);
        }
    }
}