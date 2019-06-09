namespace roundhouse.infrastructure.app.persistence
{
    using System;
    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;
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

        public static ISessionFactory build_session_factory<fluent_mappings, fluent_conventions>(string db_server, string db_name, Action<Configuration> additional_function)
        {
            logger.Debug("Building Session Factory");
            //todo: will have to figure out how to make the IPC passable
            return Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2005
                              .ConnectionString(c => c.Server(db_server).Database(db_name).TrustedConnection())
                )
                .Mappings(m =>
                {
                    m.FluentMappings.AddFromAssemblyOf<fluent_mappings>()
                        .Conventions.AddFromAssemblyOf<fluent_conventions>();
                })
                .ExposeConfiguration(additional_function)
                .BuildSessionFactory();

        }

        private static void no_operation(Configuration cfg)
        {
        }
    }
}