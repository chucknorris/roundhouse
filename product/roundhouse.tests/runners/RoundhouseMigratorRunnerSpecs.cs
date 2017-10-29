using Moq;
using roundhouse.consoles;
using roundhouse.databases;
using roundhouse.environments;
using roundhouse.infrastructure.containers;
using roundhouse.infrastructure.containers.custom;
using roundhouse.infrastructure.filesystem;
using roundhouse.infrastructure.logging;
using roundhouse.infrastructure.logging.custom;
using roundhouse.migrators;
using roundhouse.resolvers;
using roundhouse.runners;

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

            protected concern_for_migrator_runner()
            {
                configuration = new DefaultConfiguration
                {
                    EnvironmentNames = "TEST",
                    Drop = false ,
                    Silent = true
                };

                var database_mock = new Mock<Database>();
                var filesystem_mock = new Mock<FileSystemAccess>();
                var version_resolver_mock = new Mock<VersionResolver>();

                var known_folders_mock = new MockKnownFolders();

                var environment_mock = Mock.Of<EnvironmentSet>();
                
                database_migrator_mock = new Mock<DatabaseMigrator>();
                database_migrator_mock.Setup(m => m.database).Returns(database_mock.Object);

                default_database_migrator = 
                        new RoundhouseMigrationRunner(
                            configuration.RepositoryPath,
                            environment_mock,
                            known_folders_mock,
                            filesystem_mock.Object,
                            database_migrator_mock.Object,
                            version_resolver_mock.Object,
                            configuration.Silent,
                            configuration.Drop,
                            configuration.DoNotCreateDatabase,
                            configuration.WithTransaction,
                            configuration);

                var container_mock = new Mock<InversionContainer>();

                setup_logging(container_mock);

                var the_container = container_mock.Object;
                Container.initialize_with(the_container);
            }

            private static void setup_logging(Mock<InversionContainer> container_mock)
            {
                var mock_log_factory = new Mock<LogFactory>();
                var log_factory = mock_log_factory.Object;

                var logger = get_logger();

                mock_log_factory.Setup(x => x.create_logger_bound_to(typeof(RoundhouseMigrationRunner)))
                    .Returns(logger);

                container_mock.Setup(x => x.Resolve<LogFactory>())
                    .Returns(log_factory);
            }

            private static Logger get_logger()
            {
                return new TraceLogger(true);
            }

            public override void AfterEachSpec()
            {
                Container.initialize_with(null);
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
            public when_setting_do_not_alter_database(): base()
            {
                configuration.DoNotAlterDatabase = true;
            }

            public override void Context()
            {}

            public override void Because()
            {
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

            public when_not_setting_do_not_alter_database(): base()
            {
                configuration.DoNotAlterDatabase = false;
                sut.run();
            }

            public override void Context()
            {}

            public override void Because()
            {
            }

            [Observation]
            public void a_connection_to_admin_connection_is_made()
            {
                database_migrator_mock.Verify(m => m.open_admin_connection(), Times.Once);
            }
        }

    }
}