using roundhouse.databases.ravendb.commands;
using NUnit.Framework;
using Rhino.Mocks;
using roundhouse.infrastructure.app;

namespace roundhouse.databases.ravendb.tests
{
    [TestFixture]
    public class RavenDatabaseTestFixture
    {
        [Test]
        public void WhenExecutingInsertScriptRunItCreatesACommandAndExecutesIt()
        {
            // Arrange
            const string scriptToRun = @"PUT connectionString/docs/ScriptsRun/test.txt -d ""{""id"":0,""version_id"":1,""script_name"":""test.txt"",""text_of_script"":""DataToPut"",""text_hash"":""thehash"",""one_time_script"":true,""entry_date"":null,""modified_date"":null,""entered_by"":""System""}"" ";
            
            var command = MockRepository.GenerateStrictMock<IRavenCommand>();
            command.Expect(s => s.ExecuteCommand());
            command.Expect(s => s.Dispose());
            
            var factory = MockRepository.GenerateStrictMock<IRavenCommandFactory>();
            factory.Expect(s => s.CreateRavenCommand(scriptToRun)).Return(command);
            var database = new RavenDatabase {RavenCommandFactory = factory, connection_string = "connectionString"};
            // Act
            database.insert_script_run("test.txt", "DataToPut", "thehash", true, 1);

            //Assert
            command.VerifyAllExpectations();
            factory.VerifyAllExpectations();
        }

        [Test]
        public void WhenExecutingInsertScriptRunErrorItCreatesACommandAndExecutesIt()
        {
            // Arrange
            const string scriptToRun = @"PUT connectionString/docs/ScriptsRunError/test.txt -d ""{""id"":0,""repository_path"":""RepositoryPath"",""version"":""RepositoryVersion"",""script_name"":""test.txt"",""text_of_script"":""DataToPut"",""erroneous_part_of_script"":""DataToPutWithError"",""error_message"":""ErrorMessage"",""entry_date"":null,""modified_date"":null,""entered_by"":""System""}"" ";

            var command = MockRepository.GenerateStrictMock<IRavenCommand>();
            command.Expect(s => s.ExecuteCommand());
            command.Expect(s => s.Dispose());

            var factory = MockRepository.GenerateStrictMock<IRavenCommandFactory>();
            factory.Expect(s => s.CreateRavenCommand(scriptToRun)).Return(command);
            var database = new RavenDatabase {RavenCommandFactory = factory, connection_string = "connectionString"};
            // Act
            database.insert_script_run_error("test.txt", "DataToPut", "DataToPutWithError","ErrorMessage","RepositoryVersion","RepositoryPath");

            //Assert
            command.VerifyAllExpectations();
            factory.VerifyAllExpectations();
        }


        [Test]
        public void WhenRunningAScriptTheUnderlyingCommandIsExecuted()
        {
            // Arrange
            var command = MockRepository.GenerateStrictMock<IRavenCommand>();
            command.Expect(s => s.ExecuteCommand());
            command.Expect(s => s.Dispose());

            var factory = MockRepository.GenerateStrictMock<IRavenCommandFactory>();
            factory.Expect(s => s.CreateRavenCommand("test")).Return(command);
            var database = new RavenDatabase {RavenCommandFactory = factory};

            // Act
            database.run_sql("test", ConnectionType.Default);

            //Assert
            command.VerifyAllExpectations();
            factory.VerifyAllExpectations();
        }

        [Test]
        public void WhenNoRavenCommandFactoryIsSetTheDefaultIsUsed()
        {
            var database = new RavenDatabase();
            Assert.That(database.RavenCommandFactory, Is.Not.Null);
            Assert.That(database.RavenCommandFactory, Is.TypeOf<RavenCommandFactory>());
        }

        [Test]
        public void WhenClosingTheConnectionNothingHappens()
        {
            // Arrange
            var factory = MockRepository.GenerateStrictMock<IRavenCommandFactory>();
            var database = new RavenDatabase { RavenCommandFactory = factory };
            // Act
            database.close_connection();

            // Assert
            factory.VerifyAllExpectations();
        }
    }
}