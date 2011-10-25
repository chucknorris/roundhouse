using System.Collections.Generic;

namespace roundhouse.infrastructure.app.persistence
{
    using System;
    using log4net;
    using NHibernate;
    using NHibernate.Cfg;

    public class NHibernateMigrationSessionFactory
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(NHibernateMigrationSessionFactory));

        public ISessionFactory build_session_factory<fluent_mappings,fluent_conventions>(string db_server, string db_name)
        {
            return build_session_factory<fluent_mappings, fluent_conventions>(db_server, db_name, no_operation);
        }

        [Obsolete("Now that FNH is gone, prefer the single generic build_session_factory method")]
        public static ISessionFactory build_session_factory<fluent_mappings, fluent_conventions>(string db_server, string db_name, Action<Configuration> additional_function)
        {
            return build_session_factory<fluent_mappings>(db_server, db_name, additional_function);
        }

        public static ISessionFactory build_session_factory<mappings>(string db_server, string db_name, Action<Configuration> additional_function)
        {
            logger.Debug("Building Session Factory");
            //todo: will have to figure out how to make the IPC passable
            NHibernate.Cfg.Environment.BytecodeProvider = null;

            var cfg = new Configuration();
            
            var configSettings = new Dictionary<string, string>();
            
            configSettings.Add(NHibernate.Cfg.Environment.Dialect, "NHibernate.Dialect.MsSql2005Dialect");
            configSettings.Add(NHibernate.Cfg.Environment.ConnectionString, string.Format("Data Source={0};Initial Catalog={1};Trusted_Connection=True;", db_server, db_name));
            configSettings.Add(NHibernate.Cfg.Environment.ConnectionDriver, "NHibernate.Driver.SqlClientDriver");

            cfg.AddProperties(configSettings);

            add_mappings_from<mappings>(cfg);

            additional_function(cfg);

            return cfg.BuildSessionFactory();
        }

        private static void no_operation(Configuration cfg)
        {
        }


        private static void add_mappings_from<mappings>(Configuration cfg)
        {
            //pull each embedde resource out.
            var assembly = typeof (mappings).Assembly;

            foreach (var manifestResourceName in assembly.GetManifestResourceNames())
            {
                if (!manifestResourceName.Contains("orm")) continue;

                var resource = assembly.GetManifestResourceStream(manifestResourceName);
                //cfg.a
            }
            

        }
    }
}