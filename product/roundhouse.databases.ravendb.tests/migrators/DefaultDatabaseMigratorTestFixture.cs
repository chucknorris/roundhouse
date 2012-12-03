using System.Net;
using roundhouse.infrastructure.app;
using NUnit.Framework;
using Rhino.Mocks;
using roundhouse.databases;
using roundhouse.environments;
using roundhouse.migrators;

namespace roundhouse.tests.migrators
{
    [TestFixture]
    public class DefaultDatabaseMigratorTestFixture
    {
        [Test]
        public void WhenRunningTheScriptItCallsTheDatabaseToRunSql()
        {
            // Arrange
            var databaseMock = MockRepository.GenerateStrictMock<Database>();
            databaseMock.Expect(s => s.run_sql(Arg<string>.Is.Equal("ScriptTest"), Arg<ConnectionType>.Is.Anything));
            databaseMock.Expect(s => s.insert_script_run(Arg<string>.Is.Equal("script.txt"), Arg<string>.Is.Equal("ScriptTest"), Arg<string>.Is.Equal("hash"), Arg<bool>.Is.Anything, Arg<long>.Is.Equal(1)));
            databaseMock.Expect(s => s.database_name).Return("default").Repeat.Twice();
            databaseMock.Expect(s => s.server_name).Return("Localhost").Repeat.Twice();
            databaseMock.Expect(s => s.split_batch_statements).Return(false);
            

            var cryptoProviderMock = MockRepository.GenerateStrictMock<cryptography.CryptographicService>();
            cryptoProviderMock.Expect(s => s.hash(Arg<string>.Is.Anything)).Return("hash");
            
            // Act
            var migrator = new DefaultDatabaseMigrator(databaseMock, cryptoProviderMock);
            migrator.Run_Script("ScriptTest", "script.txt", true, true, 1, new DefaultEnvironment("DefaultEnvironment"), "1.0", "path", ConnectionType.Default);

            databaseMock.VerifyAllExpectations();
            cryptoProviderMock.VerifyAllExpectations();
        }

        [Test]
        public void WhenRunningTheScriptThrowsAnExceptionItCallsInsertScriptInError()
        {
            // Arrange
            var databaseMock = MockRepository.GenerateStrictMock<Database>();
            databaseMock.Expect(s => s.run_sql(Arg<string>.Is.Anything, Arg<ConnectionType>.Is.Anything)).Throw(new WebException("WebMessage"));
            databaseMock.Expect(s => s.insert_script_run_error(Arg<string>.Is.Equal("script.txt"), Arg<string>.Is.Equal("ScriptTest"),Arg<string>.Is.Anything,Arg<string>.Is.Equal("WebMessage"),Arg<string>.Is.Anything, Arg<string>.Is.Anything));
            databaseMock.Expect(s => s.database_name).Return("default").Repeat.Twice();
            databaseMock.Expect(s => s.server_name).Return("Localhost").Repeat.Twice();
            databaseMock.Expect(s => s.split_batch_statements).Return(false);
            databaseMock.Expect(s => s.close_connection());

            var cryptoProviderMock = MockRepository.GenerateStrictMock<cryptography.CryptographicService>();

            // Act
            var migrator = new DefaultDatabaseMigrator(databaseMock, cryptoProviderMock);
            var result = Assert.Throws<WebException>(() => migrator.Run_Script("ScriptTest", "script.txt", true, true,1,new DefaultEnvironment("DefaultEnvironment"), "1.0","path", ConnectionType.Default));

            Assert.That(result.Message, Is.EqualTo("WebMessage"));
            databaseMock.VerifyAllExpectations();
            cryptoProviderMock.VerifyAllExpectations();
        }

      
    }
}
