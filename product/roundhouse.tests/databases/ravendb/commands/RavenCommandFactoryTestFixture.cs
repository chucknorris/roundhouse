using MbUnit.Framework;
using roundhouse.databases.ravendb.commands;

namespace roundhouse.tests.databases.ravendb.commands
{
    [TestFixture]
    public class RavenCommandFactoryTestFixture
    {
        [RowTest]
        [Row("PUT http://localhost:8080/ -d \"data\" ", "http://localhost:8080/", "data", "RavenPutCommand")]
        [Row("PUT \r\n http://localhost:8080/ -d \r\n\"data\" \r\n", "http://localhost:8080/", "data", "RavenPutCommand")]
        [Row("POST \r\n http://localhost:8080/ -d \r\n\"data\" \r\n", "http://localhost:8080/", "data", "RavenPostCommand")]
        [Row("DELETE \r\n http://localhost:8080/ -d \r\n\"data\" \r\n", "http://localhost:8080/", "data", "RavenDeleteCommand")]
        [Row("PATCH \r\n http://localhost:8080/ -d \r\n\"data\" \r\n", "http://localhost:8080/", "data", "RavenPatchCommand")]
        public void GetRavenCommandFactoryTests(string file, string address, string data, string type)
        {
            var factory = new RavenCommandFactory();
            IRavenCommand result = factory.CreateRavenCommand(file);

            Assert.AreEqual(address, result.Address);
            Assert.AreEqual(type, result.GetType().Name);

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
            Assert.AreEqual(expectedData, ravenPutCommand.Data);
        }

        private void AssertRavenCommand(RavenPostCommand ravenPostCommand, string expectedData)
        {
            Assert.AreEqual(expectedData, ravenPostCommand.Data);
        }

        private void AssertRavenCommand(RavenPatchCommand ravenPatchCommand, string expectedData)
        {
            Assert.AreEqual(expectedData, ravenPatchCommand.Data);
        }

        [RowTest]
        [Row("http://server/", "http://localhost:8080/docs/testdocument", "http://server/docs/testdocument")]
        [Row("http://server/", "http://localhost/docs/testdocument", "http://server/docs/testdocument")]
        [Row("http://server/", "http://localhost/docs/testdocument", "http://server/docs/testdocument")]
        [Row("http://server:8080/", "http://localhost:8080/docs/testdocument", "http://server:8080/docs/testdocument")]
        [Row("http://server/databases/Condor", "http://localhost:8080/docs/testdocument", "http://server/databases/Condor/docs/testdocument")]
        [Row("http://server/databases/Condor//", "http://localhost:8080/docs/testdocument", "http://server/databases/Condor/docs/testdocument")]
        [Row("http://server with magic spaces/databases/Condor//", "http://localhost:8080/docs/testdocument", "http://server with magic spaces/databases/Condor/docs/testdocument")]
        public void AssertAddressBasedOnConnectionString(string connectionString, string address, string expectedAddress)
        {
            var ravenCommandFactory = new RavenCommandFactory();
            ravenCommandFactory.ConnectionString = connectionString;

            IRavenCommand command =
                ravenCommandFactory.CreateRavenCommand(string.Format(@"POST {0} -d ""data"" ", address));

            Assert.AreEqual(expectedAddress, command.Address);

        }
    }
}