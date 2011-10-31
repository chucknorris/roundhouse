namespace roundhouse.databases.sqlite
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using infrastructure;

    public class SqliteAdoNetProviderResolver
    {
        private readonly bool is_merged;

        public SqliteAdoNetProviderResolver()
        {
            is_merged = Assembly.GetExecutingAssembly().GetName().Name == ApplicationParameters.get_merged_assembly_name();
        }

        public void register_db_provider_factory()
        {
            var dataSet = (DataSet)ConfigurationManager.GetSection("system.data");

            var sql_client_row = dataSet.Tables[0]
                .AsEnumerable()
                .Where(x => x.Field<string>(2) == "System.Data.SQLite")
                .SingleOrDefault();

            if (sql_client_row != null) dataSet.Tables[0].Rows.Remove(sql_client_row);

            var factory_type = is_merged
                                   ? "System.Data.SQLite.SQLiteFactory, " + ApplicationParameters.get_merged_assembly_name()
                                   : "System.Data.SQLite.SQLiteFactory, System.Data.SQLite";

            dataSet.Tables[0].Rows.Add(
                "Sqlite Data Provider",
                ".Net Framework Data Provider for Sqlite",
                "System.Data.SQLite",
                factory_type);
        }

        public void enable_loading_from_merged_assembly()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => load_sqlite_data_assembly(e.Name);
        }

        private Assembly load_sqlite_data_assembly(string assembly_name)
        {
            if (is_merged && assembly_name == "System.Data.SQLite")
            {
                return Assembly.GetExecutingAssembly();
            }

            return null;
        }
    }
}