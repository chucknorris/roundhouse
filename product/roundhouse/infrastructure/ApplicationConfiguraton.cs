using roundhouse.databases;
using roundhouse.infrastructure.extensions;

namespace roundhouse.infrastructure
{
    using System;
    using System.Collections.Generic;
    using containers;
    using cryptography;
    using environments;
    using filesystem;
    using folders;
    using logging;
    using logging.custom;
    using migrators;
    using resolvers;
    using StructureMap;
    using Container=roundhouse.infrastructure.containers.Container;
    using Environment = roundhouse.environments.Environment;

    public static class ApplicationConfiguraton
    {
        public static void set_defaults_if_properties_are_not_set(ConfigurationPropertyHolder configuration_property_holder)
        {
            if (string.IsNullOrEmpty(configuration_property_holder.ServerName))
            {
                configuration_property_holder.ServerName = ApplicationParameters.default_server_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.UpFolderName))
            {
                configuration_property_holder.UpFolderName = ApplicationParameters.default_up_folder_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.DownFolderName))
            {
                configuration_property_holder.DownFolderName = ApplicationParameters.default_down_folder_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.RunFirstAfterUpFolderName))
            {
                configuration_property_holder.RunFirstAfterUpFolderName = ApplicationParameters.default_run_first_after_up_folder_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.FunctionsFolderName))
            {
                configuration_property_holder.FunctionsFolderName = ApplicationParameters.default_functions_folder_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.ViewsFolderName))
            {
                configuration_property_holder.ViewsFolderName = ApplicationParameters.default_views_folder_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.SprocsFolderName))
            {
                configuration_property_holder.SprocsFolderName = ApplicationParameters.default_sprocs_folder_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.PermissionsFolderName))
            {
                configuration_property_holder.PermissionsFolderName = ApplicationParameters.default_permissions_folder_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.SchemaName))
            {
                configuration_property_holder.SchemaName = ApplicationParameters.default_roundhouse_schema_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.ScriptsRunTableName))
            {
                configuration_property_holder.ScriptsRunTableName = ApplicationParameters.default_scripts_run_table_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.VersionTableName))
            {
                configuration_property_holder.VersionTableName = ApplicationParameters.default_version_table_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.VersionFile))
            {
                configuration_property_holder.VersionFile = ApplicationParameters.default_version_file;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.VersionXPath))
            {
                configuration_property_holder.VersionXPath = ApplicationParameters.default_version_x_path;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.EnvironmentName))
            {
                configuration_property_holder.EnvironmentName = ApplicationParameters.default_environment_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.OutputPath))
            {
                configuration_property_holder.OutputPath = ApplicationParameters.default_output_path;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.DatabaseType))
            {
                configuration_property_holder.DatabaseType = ApplicationParameters.default_database_type;
            } 
            if (configuration_property_holder.RestoreTimeout==0)
            {
                configuration_property_holder.RestoreTimeout = ApplicationParameters.default_restore_timeout;
            }
        }

        public static void build_the_container(ConfigurationPropertyHolder configuration_property_holder)
        {
            Container.initialize_with(null);
            Container.initialize_with(build_items_for_container(configuration_property_holder));
        }

        private static InversionContainer build_items_for_container(ConfigurationPropertyHolder configuration_property_holder)
        {
            //this becomes a scan
            configuration_property_holder.DatabaseType = convert_database_type_synonyms(configuration_property_holder.DatabaseType);

            ObjectFactory.Configure(cfg =>
                                    {
                                        cfg.For<ConfigurationPropertyHolder>().Use(configuration_property_holder);

                                        cfg.For<FileSystemAccess>().Use<WindowsFileSystemAccess>();
                                        cfg.For<KnownFolders>().Use(cxt => KnownFoldersBuilder.Build(cxt.GetInstance<FileSystemAccess>(), configuration_property_holder));
                                        cfg.For<LogFactory>().Use<MultipleLoggerLogFactory>();
                                        cfg.Scan(scan=>
                                                 {
                                                     scan.AssemblyContainingType<SubLogger>();
                                                     scan.AddAllTypesOf<SubLogger>();
                                                 });
                                        cfg.For<Logger>().Use<MultipleLogger>();


                                        cfg.For<Database>().Use(cxt => DatabaseFactory.Build(cxt.GetInstance<FileSystemAccess>(), configuration_property_holder));
                                        cfg.For<CryptographicService>().Use<MD5CryptographicService>();


                                        cfg.For<DatabaseMigrator>().Use<DefaultDatabaseMigrator>();

                                        cfg.For<VersionResolver>().Use(cxt =>
                                                                       {
                                                                           VersionResolver xml_version_finder = new XmlFileVersionResolver(cxt.GetInstance<FileSystemAccess>(),
                                                                           configuration_property_holder.VersionXPath,
                                                                           configuration_property_holder.VersionFile);
                                                                           VersionResolver dll_version_finder = new DllFileVersionResolver(cxt.GetInstance<FileSystemAccess>(),
                                                                                                                                           configuration_property_holder.VersionFile);
                                                                           IEnumerable<VersionResolver> resolvers = new List<VersionResolver> { xml_version_finder, dll_version_finder };

                                                                           return new ComplexVersionResolver(resolvers);
                                                                       });

                                        cfg.For<Environment>().Use<DefaultEnvironment>();
                                    });

            return new containers.custom.StructureMapContainer(ObjectFactory.Container);
        }


        private static string convert_database_type_synonyms(string database_type)
        {
            string database_type_full_name = database_type;

            switch (database_type.to_lower())
            {
                case "2008":
                case "sql2008":
                case "sqlserver2008":
                    database_type_full_name =
                        "roundhouse.databases.sqlserver2008.SqlServerDatabase, roundhouse.databases.sqlserver2008";
                    break;
                case "2005":
                case "sql2005":
                case "sqlserver2005":
                    database_type_full_name =
                       "roundhouse.databases.sqlserver2005.SqlServerDatabase, roundhouse.databases.sqlserver2005";
                    break;
                case "2000":
                case "sql2000":
                case "sqlserver2000":
                    database_type_full_name =
                      "roundhouse.databases.sqlserver2000.SqlServerDatabase, roundhouse.databases.sqlserver2000";
                    throw new NotSupportedException(
                        "Microsoft SQL Server 2000? Really? Like SNL's \"really.\" Really?! Nice try though.");
                    break;
                case "sql" :
                case "sql.net":
                case "sqlserver":
                case "sqlado.net":
                    database_type_full_name =
                       "roundhouse.databases.sqlserver.SqlServerDatabase, roundhouse.databases.sqlserver";
                    break;
                case "mysql":
                    database_type_full_name =
                      "roundhouse.databases.mysql.MySqlDatabase, roundhouse.databases.mysql";
                    break;
                case "oracle":
                    database_type_full_name =
                     "roundhouse.databases.oracle.OracleDatabase, roundhouse.databases.oracle";
                    break;
                case "oledb":
                    database_type_full_name =
                     "roundhouse.databases.oledb.OleDbDatabase, roundhouse.databases.oledb";
                    break;
            }

            return database_type_full_name;
        }
    }
}