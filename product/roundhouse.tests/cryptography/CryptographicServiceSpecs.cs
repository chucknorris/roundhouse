using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

using NUnit.Framework;
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
                SHA1 dude = SHA1.Create();
                string text_to_hash = "I want to see what the freak is going on here";
                byte[] clear_text_bytes = Encoding.UTF8.GetBytes(text_to_hash);
                byte[] cypher_bytes = dude.ComputeHash(clear_text_bytes);
                Assert.AreEqual(20, cypher_bytes.Length);
                Debug.WriteLine(cypher_bytes);
                string base_64_cypher = Convert.ToBase64String(cypher_bytes);
                Assert.AreEqual(28, base_64_cypher.Length);
                Debug.WriteLine(base_64_cypher);
            }
        }

        [TestFixture]
        public class when_using_SHA1_to_do_cryptography
        {
            private CryptographicService sha1_crypto;

            [SetUp]
            public void we_set_the_context()
            {
                sha1_crypto = new SHA1CryptographicService();
            }

            [Test]
            public void should_be_able_to_pass_text_and_get_back_a_base_64_hash_of_the_text()
            {
                string text_to_hash = "I want to see what the freak is going on here";
                string expected_hash = "HD9I1wspfgyzkm6z9eZaGKhVLuA=";
                Assert.AreEqual(expected_hash, sha1_crypto.hash(text_to_hash));
            }
        }
    }
}