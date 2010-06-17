namespace BuildDatabase
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using NHibernate;
    using NHibernate.Tool.hbm2ddl;
    using Configuration = NHibernate.Cfg.Configuration;

    internal class Program
    {
        private static string PATH_TO_SCRIPTS;

        private static void Main(string[] args)
        {
            try
            {
                string roundhouse_exe = ConfigurationManager.AppSettings["roundhouse_exe"];
                const string db_server = "(local)";
                string db_name = ConfigurationManager.AppSettings["db_name"];
                string path_to_scripts = ConfigurationManager.AppSettings["path_to_scripts"];
                PATH_TO_SCRIPTS = path_to_scripts;
                create_the_database(roundhouse_exe, db_name);
                build_database_schema(db_server, db_name);
                run_roundhouse_drop_create(roundhouse_exe, db_name, path_to_scripts);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }

        private static void create_the_database(string roundhouse_exe, string db_name)
        {
            CommandRunner.run(roundhouse_exe, String.Format("/db={0} /f={1} /silent /simple", db_name, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)), true);
        }


        private static void build_database_schema(string db_server, string db_name)
        {
            ISessionFactory sf = NHibernateSessionFactory.build_session_factory(db_server, db_name, build_schema);
        }

        private static void build_schema(Configuration cfg)
        {
            SchemaExport s = new SchemaExport(cfg);
            s.SetOutputFile(Path.Combine(PATH_TO_SCRIPTS, "Up\\0001_CreateTables_NH.sql"));
            s.Create(true, false);
        }

        private static void run_roundhouse_drop_create(string roundhouse_exe, string db_name, string path_to_scripts)
        {
            CommandRunner.run(roundhouse_exe, String.Format("/db={0} /f={1} /silent /drop", db_name, path_to_scripts), true);
            CommandRunner.run(roundhouse_exe, String.Format("/db={0} /f={1} /silent /simple", db_name, path_to_scripts), true);
        }
    }
}