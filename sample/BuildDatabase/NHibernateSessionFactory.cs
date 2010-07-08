namespace BuildDatabase
{
    using System;
    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;
    using log4net;
    using NHibernate;
    using NHibernate.Cfg;
    using SampleProject.orm;
    using SampleProject.orm.conventions;

    public class NHibernateSessionFactory
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof (NHibernateSessionFactory));

        public ISessionFactory build_session_factory(string db_server, string db_name)
        {
            return build_session_factory(db_server, db_name, no_operation);
        }

        public static ISessionFactory build_session_factory(string db_server, string db_name, Action<Configuration> additional_function)
        {
            logger.Debug("Building Session Factory");
            return Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2005
                              .ConnectionString(c => c.Server(db_server).Database(db_name).TrustedConnection())
                )
                .Mappings(m =>
                              {
                                  m.FluentMappings.AddFromAssemblyOf<SampleItemMapping>()
                                      .Conventions.AddFromAssemblyOf<PrimaryKeyConvention>();
                              })
                .ExposeConfiguration(additional_function)
                .BuildSessionFactory();
           
        }

        private static void no_operation(Configuration cfg)
        {
        }
    }
}