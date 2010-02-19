using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using MbUnit.Framework;
using roundhouse.cryptography;

namespace roundhouse.tests.cryptography
{
    public class CryptographicServiceSpecs
    {
        [TestFixture]
        public class LearnHashing
        {
            [Test]
            public void this_is_me_learning_hashing()
            {
                MD5 dude = MD5.Create();
                string text_to_hash = "I want to see what the freak is going on here";
                byte[] clear_text_bytes = Encoding.UTF8.GetBytes(text_to_hash);
                byte[] cypher_bytes = dude.ComputeHash(clear_text_bytes);
                Assert.AreEqual(16, cypher_bytes.Length);
                Debug.WriteLine(cypher_bytes);
                string base_64_cypher = Convert.ToBase64String(cypher_bytes);
                Assert.AreEqual(24, base_64_cypher.Length);
                Debug.WriteLine(base_64_cypher);
            }
        }

        [TestFixture]
        public class when_using_MD5_to_do_cryptography
        {
            private CryptographicService md5_crypto;

            [SetUp]
            public void we_set_the_context()
            {
                md5_crypto = new MD5CryptographicService();
            }

            [Test]
            public void should_be_able_to_pass_text_and_get_back_a_base_64_hash_of_the_text()
            {
                string text_to_hash = "I want to see what the freak is going on here";
                string expected_hash = "TMGPZJmBhSO5uYbf/TBqNA==";
                Assert.AreEqual(expected_hash, md5_crypto.hash(text_to_hash));
            }
        }
    }
}