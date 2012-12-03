using roundhouse.databases.ravendb.commands;
using NUnit.Framework;

namespace roundhouse.databases.ravendb.tests.commands
{
    [TestFixture]
    public class RavenCommandFactoryTestFixture
    {
        [Test]
        [TestCase("PUT http://localhost:8080/ -d \"data\" ", "http://localhost:8080/", "data", "RavenPutCommand")]
        [TestCase("PUT \r\n http://localhost:8080/ -d \r\n\"data\" \r\n", "http://localhost:8080/", "data", "RavenPutCommand")]
        [TestCase("POST \r\n http://localhost:8080/ -d \r\n\"data\" \r\n", "http://localhost:8080/", "data", "RavenPostCommand")]
        [TestCase("DELETE \r\n http://localhost:8080/ -d \r\n\"data\" \r\n", "http://localhost:8080/", "data", "RavenDeleteCommand")]
        [TestCase("PATCH \r\n http://localhost:8080/ -d \r\n\"data\" \r\n", "http://localhost:8080/", "data", "RavenPatchCommand")]
        public void GetRavenCommandFactoryTests(string file, string address, string data, string type)
        {
            var factory = new RavenCommandFactory();
            IRavenCommand result = factory.CreateRavenCommand(file);
            Assert.That(result.Address, Is.EqualTo(address));
            Assert.That(result.GetType().Name, Is.EqualTo(type));
            if (result is RavenPutCommand)
            {
                AssertRavenCommand(result as RavenPutCommand, data);
            }
            if (result is RavenPostCommand)
            {
                AssertRavenCommand(result as RavenPostCommand, data);
            }
            if (result is RavenPatchCommand)
            {
                AssertRavenCommand(result as RavenPatchCommand, data);
            }
        }

        private void AssertRavenCommand(RavenPutCommand ravenPutCommand, string expectedData)
        {
            Assert.That(ravenPutCommand.Data, Is.EqualTo(expectedData));
        }

        private void AssertRavenCommand(RavenPostCommand ravenPostCommand, string expectedData)
        {
            Assert.That(ravenPostCommand.Data, Is.EqualTo(expectedData));
        }

        private void AssertRavenCommand(RavenPatchCommand ravenPatchCommand, string expectedData)
        {
            Assert.That(ravenPatchCommand.Data, Is.EqualTo(expectedData));
        }
    }
}