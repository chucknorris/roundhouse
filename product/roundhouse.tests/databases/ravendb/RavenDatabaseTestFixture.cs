using System;
using MbUnit.Framework;
using Rhino.Mocks;
using roundhouse.databases.ravendb;
using roundhouse.databases.ravendb.commands;
using roundhouse.databases.ravendb.models;
using roundhouse.databases.ravendb.serializers;
using roundhouse.infrastructure.app;
using Version = roundhouse.model.Version;

namespace roundhouse.tests.databases.ravendb
{
    [TestFixture]
    public class RavenDatabaseTestFixture
    {
        [Test]
        public void WhenClosingTheConnectionNothingHappens()
        {
            // Arrange
            var factory = MockRepository.GenerateStrictMock<IRavenCommandFactory>();
            var database = new RavenDatabase {RavenCommandFactory = factory};
            // Act
            database.close_connection();

            // Assert
            factory.VerifyAllExpectations();
        }

        [Test]
        public void WhenExecutingInsertScriptRunErrorItCreatesACommandAndExecutesIt()
        {
            // Arrange
            const string scriptToRun =
                @"PUT connectionString/docs/ScriptsRunError/test.txt -d ""{""id"":0,""repository_path"":""RepositoryPath"",""version"":""RepositoryVersion"",""script_name"":""test.txt"",""text_of_script"":""DataToPut"",""erroneous_part_of_script"":""DataToPutWithError"",""error_message"":""ErrorMessage"",""entry_date"":null,""modified_date"":null,""entered_by"":""System""}"" ";

            var command = MockRepository.GenerateStrictMock<IRavenCommand>();
            command.Expect(s => s.ExecuteCommand()).Return(string.Empty);
            command.Expect(s => s.Dispose());

            var factory = MockRepository.GenerateStrictMock<IRavenCommandFactory>();
            factory.Expect(s => s.CreateRavenCommand(scriptToRun)).Return(command);
            var database = new RavenDatabase {RavenCommandFactory = factory, connection_string = "connectionString"};
            // Act
            database.insert_script_run_error("test.txt", "DataToPut", "DataToPutWithError", "ErrorMessage",
                                             "RepositoryVersion", "RepositoryPath");

            //Assert
            command.VerifyAllExpectations();
            factory.VerifyAllExpectations();
        }

        [Test]
        public void WhenExecutingInsertScriptRunItCreatesACommandAndExecutesIt()
        {
            // Arrange
            const string scriptToRun =
                @"PUT connectionString/docs/ScriptsRun/test.txt -d ""{""id"":0,""version_id"":1,""script_name"":""test.txt"",""text_of_script"":""DataToPut"",""text_hash"":""thehash"",""one_time_script"":true,""entry_date"":null,""modified_date"":null,""entered_by"":""System""}"" ";

            var command = MockRepository.GenerateStrictMock<IRavenCommand>();
            command.Expect(s => s.ExecuteCommand()).Return(string.Empty);
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
        public void WhenGettingTheLatestVersionOfARepositoryPathThatDoesntExistNullIsReturned()
        {
            // Arrange
            var command = MockRepository.GenerateStrictMock<IRavenCommand>();
            command.Expect(s => s.ExecuteCommand()).Return("something");
            command.Expect(s => s.Dispose());

            var factory = MockRepository.GenerateStrictMock<IRavenCommandFactory>();
            factory.Expect(s => s.CreateRavenCommand("GET connectionString/docs/Version")).Return(command);

            var versionDocument = new VersionDocument();
            versionDocument.Versions.Add(new Version {version = "12", repository_path = "pathOther"});


            var serializer = MockRepository.GenerateStrictMock<ISerializer>();
            serializer.Expect(s => s.DeserializeObject<VersionDocument>("something")).Return(versionDocument);
            var database = new RavenDatabase
                               {
                                   RavenCommandFactory = factory,
                                   connection_string = "connectionString",
                                   Serializer = serializer
                               };

            // Act
            string result = database.get_version("path");

            //Assert
            Assert.IsNull(result);

            command.VerifyAllExpectations();
            factory.VerifyAllExpectations();
        }

        [Test]
        public void WhenGettingTheLatestVersionReturnsMultipleVersionModelsTheLastModifiedIsReturned()
        {
            // Arrange
            var command = MockRepository.GenerateStrictMock<IRavenCommand>();
            command.Expect(s => s.ExecuteCommand()).Return("something");
            command.Expect(s => s.Dispose());

            var factory = MockRepository.GenerateStrictMock<IRavenCommandFactory>();
            factory.Expect(s => s.CreateRavenCommand("GET connectionString/docs/Version")).Return(command);

            var versionDocument = new VersionDocument();
            versionDocument.Versions.Add(new Version
                                             {
                                                 version = "12",
                                                 repository_path = "path",
                                                 modified_date = new DateTime(2012, 1, 1)
                                             });
            versionDocument.Versions.Add(new Version
                                             {
                                                 version = "13",
                                                 repository_path = "path",
                                                 modified_date = new DateTime(2012, 1, 2)
                                             });


            var serializer = MockRepository.GenerateStrictMock<ISerializer>();
            serializer.Expect(s => s.DeserializeObject<VersionDocument>("something")).Return(versionDocument);
            var database = new RavenDatabase
                               {
                                   RavenCommandFactory = factory,
                                   connection_string = "connectionString",
                                   Serializer = serializer
                               };

            // Act
            string result = database.get_version("path");

            //Assert
            Assert.AreEqual("13", result);

            command.VerifyAllExpectations();
            factory.VerifyAllExpectations();
        }

        [Test]
        public void
            WhenGettingTheLatestVersionReturnsMultipleVersionModelsTheLastModifiedIsReturnedEvenIfItIsTheFirstInTheList()
        {
            // Arrange
            var command = MockRepository.GenerateStrictMock<IRavenCommand>();
            command.Expect(s => s.ExecuteCommand()).Return("something");
            command.Expect(s => s.Dispose());

            var factory = MockRepository.GenerateStrictMock<IRavenCommandFactory>();
            factory.Expect(s => s.CreateRavenCommand("GET connectionString/docs/Version")).Return(command);

            var versionDocument = new VersionDocument();
            versionDocument.Versions.Add(new Version
                                             {
                                                 version = "15",
                                                 repository_path = "path",
                                                 modified_date = new DateTime(2012, 1, 4)
                                             });
            versionDocument.Versions.Add(new Version
                                             {
                                                 version = "12",
                                                 repository_path = "path",
                                                 modified_date = new DateTime(2012, 1, 1)
                                             });
            versionDocument.Versions.Add(new Version
                                             {
                                                 version = "13",
                                                 repository_path = "path",
                                                 modified_date = new DateTime(2012, 1, 2)
                                             });


            var serializer = MockRepository.GenerateStrictMock<ISerializer>();
            serializer.Expect(s => s.DeserializeObject<VersionDocument>("something")).Return(versionDocument);
            var database = new RavenDatabase
                               {
                                   RavenCommandFactory = factory,
                                   connection_string = "connectionString",
                                   Serializer = serializer
                               };

            // Act
            string result = database.get_version("path");

            //Assert
            Assert.AreEqual("15", result);

            command.VerifyAllExpectations();
            factory.VerifyAllExpectations();
        }

        [Test]
        public void WhenGettingTheLatestVersionThatoneIsReturned()
        {
            // Arrange
            var command = MockRepository.GenerateStrictMock<IRavenCommand>();
            command.Expect(s => s.ExecuteCommand()).Return("something");
            command.Expect(s => s.Dispose());

            var factory = MockRepository.GenerateStrictMock<IRavenCommandFactory>();
            factory.Expect(s => s.CreateRavenCommand("GET connectionString/docs/Version")).Return(command);

            var versionDocument = new VersionDocument();
            versionDocument.Versions.Add(new Version {version = "12", repository_path = "path"});


            var serializer = MockRepository.GenerateStrictMock<ISerializer>();
            serializer.Expect(s => s.DeserializeObject<VersionDocument>("something")).Return(versionDocument);
            var database = new RavenDatabase
                               {
                                   RavenCommandFactory = factory,
                                   connection_string = "connectionString",
                                   Serializer = serializer
                               };

            // Act
            string result = database.get_version("path");

            //Assert
            Assert.AreEqual("12", result);

            command.VerifyAllExpectations();
            factory.VerifyAllExpectations();
        }

        [Test]
        public void WhenNoRavenCommandFactoryIsSetTheDefaultIsUsed()
        {
            var database = new RavenDatabase();
            Assert.IsNotNull(database.RavenCommandFactory);
            Assert.IsInstanceOfType(typeof(RavenCommandFactory), database.RavenCommandFactory);
        }

        [Test]
        public void WhenRunningAScriptTheUnderlyingCommandIsExecuted()
        {
            // Arrange
            var command = MockRepository.GenerateStrictMock<IRavenCommand>();
            command.Expect(s => s.ExecuteCommand()).Return(string.Empty);
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
    }
}