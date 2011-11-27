using System.Threading;

namespace roundhouse
{
    using System;
    using folders;
    using infrastructure.app;
    using infrastructure.app.logging;
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
            configuration = new consoles.DefaultConfiguration();
        }

        /// <summary>
        /// This is an optional helper to give you the correct settings for a logger. You can still set this in the set by calling propConfig.Logger without having to call this method.
        /// </summary>
        /// <param name="logger">This is the logger you want RoundhousE to also use.</param>
        /// <returns></returns>
        public Migrate SetCustomLogging(Logger logger)
        {
            return Set(c => c.Logger = logger);
        }

        /// <summary>
        /// Set your options for running rh here. It looks like Set(c => {c.DatabaseName = "bob"; c.ServerName = "(local)";}).Run();
        /// </summary>
        /// <param name="propConfig">The configuration to set</param>
        /// <returns>Itself so you can chain each of these</returns>
        public Migrate Set(Action<ConfigurationPropertyHolder> propConfig)
        {
            propConfig.Invoke(configuration);
            return this;
        }

        public ConfigurationPropertyHolder GetConfiguration()
        {
            return configuration;
        }

        /// <summary>
        /// Call this method to run the migrator after you have set the options.
        /// </summary>
        public void Run()
        {
            ApplicationConfiguraton.set_defaults_if_properties_are_not_set(configuration);
            ApplicationConfiguraton.build_the_container(configuration);

            RoundhouseMigrationRunner migrator = new RoundhouseMigrationRunner(
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

        /// <summary>
        /// Call this method to run a drop/create migration (instead of just a normal run) after you have set the options. This does not work in conjunction with the restore.
        /// </summary>
        /// <remarks>This is usually used during initial development</remarks>
        public void RunDropCreate()
        {
            if (configuration.Restore) throw new ApplicationException("You cannot use Drop/Create with Restore set to true.");
            configuration.Drop = true;
            Run();
            configuration.Drop = false;
            Run();
        }

        /// <summary>
        /// Call this method to run a restore migration (instead of just a normal run) after you have set the options. This is a helper method - you can also use the normal Run() with the restore set to true.
        /// </summary>
        /// <remarks>This is usually used during maintenance development (after production)</remarks>
        public void RunRestore()
        {
            configuration.Restore = true;
            if (string.IsNullOrEmpty(configuration.RestoreFromPath)) throw new ApplicationException("You must set RestoreFromPath in the configuration.");

            Run();
        }

    }

}