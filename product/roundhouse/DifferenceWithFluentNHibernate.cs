//namespace roundhouse
//{
//    using System.IO;
//    using System.Reflection;
//    using System.Text;
//    using infrastructure.app;
//    using infrastructure.persistence;
//    using NHibernate.Cfg;
//    using NHibernate.Tool.hbm2ddl;

//    public class DifferenceWithFluentNHibernate
//    {
//        private static string path_to_sql_scripts_up_folder;
//        private static string name_of_script_to_create;

//        /// <summary>
//        /// Set up your migrator and call this to generate a diff file. Known limitations - will not detect size changes or renames. In other words, destructive changes will need to be done by hand.
//        /// </summary>
//        /// <param name="diffingType">Are you in greenfield development or have you went to production (maintenance)? Do you want it to restore during maintenance mode?</param>
//        /// <param name="nameOfOutputScriptToCreateOrReplace">This is something like 0001_CreateTables.sql. This will end up in your up folder, assuming you have set up your migrator configuration correctly.</param>
//        /// <param name="databaseMigrator">The Migrator to use when running.</param>
//        /// <param name="mappingsAssembly">This is the assembly that contains your mapping files.</param>
//        /// <param name="conventionsAssembly">This is the assembly that contains your conventions. If you do not have conventions set up, just pass null. It will use the mappingsAssembly</param>
//        public void Run(RoundhousEFluentNHDiffingType diffingType, string nameOfOutputScriptToCreateOrReplace, Migrate databaseMigrator, Assembly mappingsAssembly, Assembly conventionsAssembly)
//        {
//            name_of_script_to_create = nameOfOutputScriptToCreateOrReplace;
//            var configuration = databaseMigrator.GetConfiguration();
//            configuration.Silent = true;
//            configuration.Restore = false;
//            ApplicationConfiguraton.set_defaults_if_properties_are_not_set(configuration);
//            path_to_sql_scripts_up_folder = Path.Combine(configuration.SqlFilesDirectory, configuration.UpFolderName);

//            switch (diffingType)
//            {
//                case RoundhousEFluentNHDiffingType.InitialDevelopment:
//                    run_initial_database_setup(databaseMigrator, configuration, mappingsAssembly, conventionsAssembly);
//                    break;
//                case RoundhousEFluentNHDiffingType.Maintenance:
//                    run_maintenance_database_setup(false, databaseMigrator, configuration, mappingsAssembly, conventionsAssembly);
//                    break;
//                case RoundhousEFluentNHDiffingType.MaintenanceWithRestore:
//                    run_maintenance_database_setup(true, databaseMigrator, configuration, mappingsAssembly, conventionsAssembly);
//                    break;
//            }
//        }

//        // initial database setup 

//        private void run_initial_database_setup(Migrate migrator, ConfigurationPropertyHolder configuration, Assembly mappings_assembly, Assembly conventions_assembly)
//        {

//            var files_directory = configuration.SqlFilesDirectory;
//            configuration.SqlFilesDirectory = ".";

//            migrator.Run();

//            generate_database_schema(configuration.DatabaseName, mappings_assembly, conventions_assembly);
            

//            configuration.SqlFilesDirectory = files_directory;
//            migrator.RunDropCreate();
//        }

//        private void generate_database_schema(string database_name,Assembly mappings_assembly,Assembly conventions_assembly)
//        {
//            DifferencingNHibernateSessionFactory.build_session_factory(database_name, mappings_assembly, conventions_assembly, generate_the_schema);
//        }

//        private void generate_the_schema(Configuration cfg)
//        {
//            var s = new SchemaExport(cfg);
//            s.SetOutputFile(Path.Combine(path_to_sql_scripts_up_folder, name_of_script_to_create));
//            s.Create(true, false);
//        }

//        // maintenance database setup

//        private void run_maintenance_database_setup(bool restoring_the_database, Migrate migrator, ConfigurationPropertyHolder configuration, Assembly mappings_assembly, Assembly conventions_assembly)
//        {
//            if (restoring_the_database)
//            {
//                configuration.Restore = true;
//                migrator.RunRestore();
//            }

//            upgrade_database_schema(configuration.DatabaseName, mappings_assembly, conventions_assembly);

//            configuration.Restore = false;
//            migrator.Run();
//        }

//        private void upgrade_database_schema(string database_name,Assembly mappings_assembly,Assembly conventions_assembly)
//        {
//            DifferencingNHibernateSessionFactory.build_session_factory(database_name, mappings_assembly, conventions_assembly, update_the_schema);
//        }

//        private void update_the_schema(Configuration cfg)
//        {
//            var s = new SchemaUpdate(cfg);
//            var sb = new StringBuilder();
//            s.Execute(x => sb.Append(x), false);
//            var updateScriptFileName = Path.Combine(path_to_sql_scripts_up_folder, name_of_script_to_create);
//            if (File.Exists(updateScriptFileName))
//            {
//                File.Delete(updateScriptFileName);
//            }
//            File.WriteAllText(updateScriptFileName, sb.ToString());
//        }
//    }
//}