using Moq;
using roundhouse.consoles;
using roundhouse.databases;
using roundhouse.environments;
using roundhouse.folders;
using roundhouse.infrastructure.containers;
using roundhouse.infrastructure.filesystem;
using roundhouse.migrators;
using roundhouse.resolvers;
using roundhouse.runners;
using roundhouse.tests.migrators;
using Should;

namespace roundhouse.tests.runners
{
    public class RoundhouseMigratorRunnerSpecs
    {
        public abstract class concern_for_migrator_runner : TinySpec<RoundhouseMigrationRunner>
        {
            protected object result;
            protected DefaultEnvironment environment;
            protected readonly DefaultConfiguration configuration;
            private RoundhouseMigrationRunner default_database_migrator;

            protected Mock<DatabaseMigrator> database_migrator_mock;
            protected DatabaseMigrator database_migrator;

            protected concern_for_migrator_runner()
            {
                configuration = new DefaultConfiguration
                {
                    EnvironmentName = "TEST",
                    Drop = false 
                };

                var database_mock = new Mock<Database>();
                var filesystem_mock = new Mock<FileSystemAccess>();
                var version_resolver_mock = new Mock<VersionResolver>();

                var known_folders_mock = new MockKnownFolders();

                var environment_mock = Mock.Of<Environment>();
                
                database_migrator_mock = new Mock<DatabaseMigrator>();
                database_migrator_mock.Setup(m => m.database).Returns(database_mock.Object);
                database_migrator = database_migrator_mock.Object;

                default_database_migrator = 
                        new RoundhouseMigrationRunner(
                            configuration.RepositoryPath,
                            environment_mock,
                            known_folders_mock,
                            filesystem_mock.Object,
                            database_migrator,
                            version_resolver_mock.Object,
                            configuration.Silent,
                            configuration.Drop,
                            configuration.DoNotCreateDatabase,
                            configuration.WithTransaction,
                            configuration.RecoveryModeSimple,
                            configuration);
            }

            public override void Context()
            {
                environment = new DefaultEnvironment(configuration);
            }

            protected override RoundhouseMigrationRunner sut
            {
                get { return default_database_migrator;}
                set { default_database_migrator = value; }
            }
        }

        [Concern(typeof(RoundhouseMigrationRunner))]
        public class when_setting_do_not_alter_database : concern_for_migrator_runner
        {
            public override void Because()
            {
                configuration.DoNotAlterDatabase = true;
                sut.run();
            }

            [Observation]
            public void no_connection_to_admin_connection_is_made()
            {
                database_migrator_mock.Verify(m => m.open_admin_connection(), Times.Never);
            }
        }

        [Concern(typeof(RoundhouseMigrationRunner))]
        public class when_not_setting_do_not_alter_database : concern_for_migrator_runner
        {
            public override void Because()
            {
                configuration.DoNotAlterDatabase = false;
                sut.run();
            }

            [Observation]
            public void a_connection_to_admin_connection_is_made()
            {
                database_migrator_mock.Verify(m => m.open_admin_connection(), Times.Once);
            }
        }

    }
}