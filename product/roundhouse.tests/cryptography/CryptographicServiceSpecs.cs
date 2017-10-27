using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

using NUnit.Framework;
using roundhouse.cryptography;
using System.Threading;
using System.Threading.Tasks;

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



        [TestFixture]
        public class when_using_an_unofficial_md5_implementation
        {
            const int passes = 2000;
            const int max_len = 10240;

            private int seed = Environment.TickCount;

            private ThreadLocal<Random> _rng;
            private ThreadLocal<byte[]> _bytes = new ThreadLocal<byte[]>(() => new byte[max_len]);
            private ThreadLocal<LocalMD5Implementation> _unofficial = new ThreadLocal<LocalMD5Implementation>(() => new LocalMD5Implementation());
            private ThreadLocal<MD5> _official = new ThreadLocal<MD5>(() => MD5.Create());

            [SetUp]
            public void we_set_the_context()
            {
                _rng = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));
            }

            [Test]
            public void should_have_parity_with_official_implementation()
            {
                Parallel.For(0, passes, i => {
                    _rng.Value.NextBytes(_bytes.Value);
                    var len = _rng.Value.Next(1, max_len + 1);
                    var h1 = _official.Value.ComputeHash(_bytes.Value, 0, len);
                    var h2 = _unofficial.Value.ComputeHash(_bytes.Value, 0, len);
                    CollectionAssert.AreEqual(h1, h2);
                });
            }
        }
    }
}