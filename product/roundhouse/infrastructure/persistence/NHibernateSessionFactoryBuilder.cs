namespace roundhouse.infrastructure.persistence
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using app;
    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;
    using loaders;
    using logging;
    using NHibernate;
    using NHibernate.Cfg;
    using NHibernate.Event;
    using Environment = NHibernate.Cfg.Environment;

    public class NHibernateSessionFactoryBuilder
    {
        private readonly ConfigurationPropertyHolder configuration_holder;
        private readonly Dictionary<string, Func<IPersistenceConfigurer>> func_dictionary;
        private const string proxy_factory = Environment.ProxyFactoryFactoryClass;
        private const string proxy_factory_name = "NHibernate.ByteCode.Castle.ProxyFactoryFactory";

        public NHibernateSessionFactoryBuilder(ConfigurationPropertyHolder config)
        {
            configuration_holder = config;
            func_dictionary = new Dictionary<string, Func<IPersistenceConfigurer>>();
            func_dictionary.Add("roundhouse.databases.sqlserver.SqlServerDatabase, roundhouse.databases.sqlserver",
                                () => MsSqlConfiguration.MsSql2005.ConnectionString(configuration_holder.ConnectionString));
            func_dictionary.Add("roundhouse.databases.sqlserver2000.SqlServerDatabase, roundhouse.databases.sqlserver2000",
                                () => MsSqlConfiguration.MsSql2000.ConnectionString(configuration_holder.ConnectionString));
            func_dictionary.Add("roundhouse.databases.mysql.MySqlDatabase, roundhouse.databases.mysql",
                                () => MySQLConfiguration.Standard.ConnectionString(configuration_holder.ConnectionString));
            func_dictionary.Add("roundhouse.databases.oracle.OracleDatabase, roundhouse.databases.oracle",
                                () => OracleClientConfiguration.Oracle9.ConnectionString(configuration_holder.ConnectionString));
            func_dictionary.Add("roundhouse.databases.access.AccessDatabase, roundhouse.databases.access",
                                () => JetDriverConfiguration.Standard.ConnectionString(configuration_holder.ConnectionString));
            func_dictionary.Add("roundhouse.databases.sqlite.SQLiteDatabase, roundhouse.databases.sqlite",
                                () => SQLiteConfiguration.Standard.ConnectionString(configuration_holder.ConnectionString));
            func_dictionary.Add("roundhouse.databases.postgresql.PostgreSQLDatabase, roundhouse.databases.postgresql",
                                () => PostgreSQLConfiguration.Standard.ConnectionString(configuration_holder.ConnectionString));
            // merged
            string merged_assembly_name = ApplicationParameters.get_merged_assembly_name();
            func_dictionary.Add("roundhouse.databases.sqlserver.SqlServerDatabase, " + merged_assembly_name,
                                () => MsSqlConfiguration.MsSql2005.ConnectionString(configuration_holder.ConnectionString));
            func_dictionary.Add("roundhouse.databases.sqlserver2000.SqlServerDatabase, " + merged_assembly_name,
                                () => MsSqlConfiguration.MsSql2000.ConnectionString(configuration_holder.ConnectionString));
            func_dictionary.Add("roundhouse.databases.mysql.MySqlDatabase, " + merged_assembly_name,
                                () => MySQLConfiguration.Standard.ConnectionString(configuration_holder.ConnectionString));
            func_dictionary.Add("roundhouse.databases.oracle.OracleDatabase, " + merged_assembly_name,
                                () => OracleClientConfiguration.Oracle9.ConnectionString(configuration_holder.ConnectionString));
            func_dictionary.Add("roundhouse.databases.access.AccessDatabase, " + merged_assembly_name,
                                () => JetDriverConfiguration.Standard.ConnectionString(configuration_holder.ConnectionString));
            func_dictionary.Add("roundhouse.databases.sqlite.SQLiteDatabase, " + merged_assembly_name,
                                () => SQLiteConfiguration.Standard.ConnectionString(configuration_holder.ConnectionString));
            func_dictionary.Add("roundhouse.databases.postgresql.PostgreSQLDatabase, " + merged_assembly_name,
                                () => PostgreSQLConfiguration.Standard.ConnectionString(configuration_holder.ConnectionString));
        }

        public ISessionFactory build_session_factory()
        {
            return build_session_factory(no_operation);
        }

        public ISessionFactory build_session_factory(Action<Configuration> additional_function)
        {
            string top_namespace = configuration_holder.DatabaseType.Substring(0, configuration_holder.DatabaseType.IndexOf(','));
            top_namespace = top_namespace.Substring(0, top_namespace.LastIndexOf('.'));
            string assembly_name = configuration_holder.DatabaseType.Substring(configuration_holder.DatabaseType.IndexOf(',') + 1);

            try
            {
                string key = configuration_holder.DatabaseType.Substring(0, configuration_holder.DatabaseType.IndexOf(',')) + ", " +
                             ApplicationParameters.get_merged_assembly_name();
                return build_session_factory(func_dictionary[key](), DefaultAssemblyLoader.load_assembly(ApplicationParameters.get_merged_assembly_name()),
                                             top_namespace, additional_function);
            }
            catch (Exception)
            {
                return build_session_factory(func_dictionary[configuration_holder.DatabaseType](), DefaultAssemblyLoader.load_assembly(assembly_name),
                                             top_namespace, additional_function);
            }
        }

        public ISessionFactory build_session_factory(IPersistenceConfigurer db_configuration, Assembly assembly, string top_namespace,
                                                     Action<Configuration> additional_function)
        {
            Log.bound_to(this).log_a_debug_event_containing("Building Session Factory");
            var config = Fluently.Configure()
                .Database(db_configuration)
                .Mappings(m => {
                              m.FluentMappings.Add(assembly.GetType(top_namespace + ".orm.VersionMapping", true, true))
                                  .Add(assembly.GetType(top_namespace + ".orm.ScriptsRunMapping", true, true))
                                  .Add(assembly.GetType(top_namespace + ".orm.ScriptsRunErrorMapping", true, true));
                              //.Conventions.AddAssembly(assembly);
                              //m.HbmMappings.AddFromAssembly(assembly);
                          })
                .ExposeConfiguration(cfg => {
                                         string proxy_factory_location = proxy_factory_name + ", " + ApplicationParameters.get_merged_assembly_name();

                                         if (!ApplicationParameters.is_type_merged_in_this_assembly(proxy_factory_name))
                                             proxy_factory_location = proxy_factory_name + ", NHibernate.ByteCode.Castle";

                                         if (cfg.Properties.ContainsKey(proxy_factory))
                                         {
                                             cfg.Properties[proxy_factory] = proxy_factory_location;
                                         }
                                         else
                                         {
                                             cfg.Properties.Add(proxy_factory, proxy_factory_location);
                                         }

                                         cfg.SetListener(ListenerType.PreInsert, new AuditEventListener());
                                         cfg.SetListener(ListenerType.PreUpdate, new AuditEventListener());
                                     })
                .ExposeConfiguration(additional_function);

            return config.BuildSessionFactory();
        }

        public NHibernateSessionFactoryBuilder() {}

        private static void no_operation(Configuration cfg) {}
    }
}