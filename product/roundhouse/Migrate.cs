namespace roundhouse
{
    using System;
    using folders;
    using infrastructure.app;
    using infrastructure.containers;
    using infrastructure.filesystem;
    using infrastructure.logging;
    using migrators;
    using resolvers;
    using runners;
    using Environment = roundhouse.environments.Environment;
    
    public class Migrate
    {
        private readonly ConfigurationPropertyHolder configuration;

        public Migrate()
        {
            configuration = new consoles.ConsoleConfiguration();
        }

        public Migrate SetCustomLogging(Logger logger)
        {
            return Set(c => c.Logger = logger);
        }

        /// <summary>
        /// Set your options for running rh here. It looks like Set(c => {c.DatabaseName = "bob"; c.ServerName = "(local)";}).Run();
        /// </summary>
        /// <param name="propConfig">the</param>
        /// <returns>Itself so you can chain each of these</returns>
        public Migrate Set(Action<ConfigurationPropertyHolder> propConfig)
        {
            propConfig.Invoke(configuration);
            return this;
        }
        
        /// <summary>
        /// Call this method to run the migrator after all of your other options are set.
        /// </summary>
        public void Run()
        {
            ApplicationConfiguraton.set_defaults_if_properties_are_not_set(configuration);
            ApplicationConfiguraton.build_the_container(configuration);

           var migrator =  new RoundhouseMigrationRunner(
               configuration.RepositoryPath,
               Container.get_an_instance_of<Environment>(),
               Container.get_an_instance_of<KnownFolders>(),
               Container.get_an_instance_of<FileSystemAccess>(),
               Container.get_an_instance_of<DatabaseMigrator>(),
               Container.get_an_instance_of<VersionResolver>(),
               configuration.Silent,
               configuration.Drop,
               configuration.DoNotCreateDatabase,
               configuration.WithTransaction,
               configuration.RecoveryModeSimple, 
               configuration);

            migrator.run();
        }
    
    }

}