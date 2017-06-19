namespace roundhouse.tests.integration.infrastructure.persistence
{
    using System.IO;
    using System.Text;
    using consoles;
    using NHibernate.Cfg;
    using NHibernate.Tool.hbm2ddl;
    using roundhouse.infrastructure.app;
    using roundhouse.infrastructure.persistence;

    public class NHibernateSessionFactorySpecs
    {
        public abstract class concern_for_NHibernateSessionFactory : TinySpec<NHibernateSessionFactoryBuilder>
        {
            protected static ConfigurationPropertyHolder config;
        }

        [Concern(typeof(NHibernateSessionFactoryBuilder))]
        public class when_nhibernate_session_factory_is_created_for_sql_server : concern_for_NHibernateSessionFactory
        {
            protected object result;

            public when_nhibernate_session_factory_is_created_for_sql_server()
            {
                config = new DefaultConfiguration
                {
                    DatabaseType = "roundhouse.databases.sqlserver.SqlServerDatabase, roundhouse.databases.sqlserver",
                    ConnectionString = "Server=(local);initial catalog=TestRoundhousE;Integrated Security=SSPI;"
                };
                sut = new NHibernateSessionFactoryBuilder(config);
            }

            public override void Context(){}

            protected override NHibernateSessionFactoryBuilder sut { get; }

            public override void Because() => result = sut.build_session_factory(get_schema_export);

            private static void get_schema_export(Configuration cfg)
            {
                build_schema(cfg);
                int i = 0;
            }

            [Observation]
            public void should_build_the_session_correctly()
            {
                //
            }

            private static void build_schema(Configuration cfg)
            {
                string sql_script_file = Path.Combine(".\\", "insert.sql");
                //if (!UPDATING)
                //{
                SchemaExport s = new SchemaExport(cfg);
                s.SetOutputFile(sql_script_file);
                s.Create(true, false);
                //}
                //else
                //{
                sql_script_file = Path.Combine(".\\", "update.sql");
                SchemaUpdate update = new SchemaUpdate(cfg);
                StringBuilder sb = new StringBuilder();
                //update.Execute(false, true);
                update.Execute(schema => sb.AppendLine(schema), false);

                File.WriteAllText(sql_script_file, sb.ToString());
                //}
            }
        }
    }
}