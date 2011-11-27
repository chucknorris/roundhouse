namespace roundhouse.databases.sqlite
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using infrastructure;
    using infrastructure.extensions;
    using infrastructure.logging;

    public class SqliteAdoNetProviderResolver
    {
        private readonly bool is_merged;

        public SqliteAdoNetProviderResolver()
        {
            is_merged = Assembly.GetExecutingAssembly().GetName().Name == ApplicationParameters.get_merged_assembly_name();
        }

        public string output_path { get; set; }

        public void register_db_provider_factory()
        {
            var dataSet = (DataSet)ConfigurationManager.GetSection("system.data");

            var sql_client_row = dataSet.Tables[0]
                .AsEnumerable()
                .Where(x => x.Field<string>(2) == "System.Data.SQLite")
                .SingleOrDefault();

            if (sql_client_row != null) dataSet.Tables[0].Rows.Remove(sql_client_row);

            dataSet.Tables[0].Rows.Add(
                "Sqlite Data Provider",
                ".Net Framework Data Provider for Sqlite",
                "System.Data.SQLite",
               "System.Data.SQLite.SQLiteFactory, System.Data.SQLite");
        }

        public void enable_loading_from_merged_assembly()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => load_sqlite_data_assembly(e.Name);
        }

        private Assembly load_sqlite_data_assembly(string assembly_name)
        {
            //http://stackoverflow.com/questions/6616034/how-can-i-merge-system-data-sqlite-into-a-single-executable-program
            //http://blogs.msdn.com/b/microsoft_press/archive/2010/02/03/jeffrey-richter-excerpt-2-from-clr-via-c-third-edition.aspx
            //http://elegantcode.com/2011/04/02/dynamically-load-embedded-assemblies-because-ilmerge-appeared-to-be-out/
            if (assembly_name.to_lower().StartsWith("system.data.sqlite"))
            {
                //would like to use file_system here...
                string resources_path = Path.Combine(output_path, "resources");
                string sqlite_file = Path.Combine(resources_path, "System.Data.SQLite.dll");
                FileInfo sqlite_dll = new FileInfo(sqlite_file);
                Log.bound_to(this).log_a_debug_event_containing("Loading System.Data.SQLite.dll from '{0}'. If it doesn't exist then it will be created from '{1}'.", sqlite_file, ApplicationParameters.sqlite_dll_resource);

                if (!sqlite_dll.Exists)
                {
                    if (!Directory.Exists(resources_path)) Directory.CreateDirectory(resources_path);

                    Assembly executing_assembly = Assembly.GetExecutingAssembly();

                    using (FileStream fs = sqlite_dll.OpenWrite())
                    using (Stream resource_stream = executing_assembly.GetManifestResourceStream(ApplicationParameters.sqlite_dll_resource))
                    {
                        const int size = 4096;
                        byte[] bytes = new byte[size];
                        int number_of_bytes;
                        while ((number_of_bytes = resource_stream.Read(bytes, 0, size)) > 0)
                        {
                            fs.Write(bytes, 0, number_of_bytes);
                        }
                        fs.Flush();
                        fs.Close();
                        resource_stream.Close();
                    }
                }


                return Assembly.LoadFrom(sqlite_file);
            }

            return null;
        }
    }
}