namespace roundhouse.tests.integration.infrastructure.persistence
{
    using System.IO;
    using System.Text;
    using bdddoc.core;
    using consoles;
    using developwithpassion.bdd.contexts;
    using developwithpassion.bdd.mbunit.standard;
    using developwithpassion.bdd.mbunit.standard.observations;
    using NHibernate.Cfg;
    using NHibernate.Tool.hbm2ddl;
    using roundhouse.infrastructure.app;
    using roundhouse.infrastructure.persistence;

    public class NHibernateSessionFactorySpecs
    {
        public abstract class concern_for_NHibernateSessionFactory : observations_for_a_sut_without_a_contract<NHibernateSessionFactoryBuilder>
        {
            protected static ConfigurationPropertyHolder config;
        }

        [Concern(typeof(NHibernateSessionFactoryBuilder))]
        public class when_nhibernate_session_factory_is_created_for_sql_server : concern_for_NHibernateSessionFactory
        {
            protected static object result;

            context c = () =>
                            {
                                config = new DefaultConfiguration();
                                config.DatabaseType = "roundhouse.databases.sqlserver.SqlServerDatabase, roundhouse.databases.sqlserver";
                                config.ConnectionString = "Server=(local);initial catalog=TestRoundhousE;Integrated Security=SSPI;";
                                provide_a_basic_sut_constructor_argument(config);
                            };

            because b = () => result = sut.build_session_factory(get_schema_export);

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