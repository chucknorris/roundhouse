namespace roundhouse.databases.mysql
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using infrastructure;

    public class MySqlAdoNetProviderResolver
    {
        private readonly bool is_merged;

        public MySqlAdoNetProviderResolver()
        {
            is_merged = Assembly.GetExecutingAssembly().GetName().Name == ApplicationParameters.get_merged_assembly_name();
        }

        public void register_db_provider_factory()
        {
            var dataSet = (DataSet) ConfigurationManager.GetSection("system.data");

            var my_sql_client_row = dataSet.Tables[0]
                .AsEnumerable()
                .Where(x => x.Field<string>(2) == "MySql.Data.MySqlClient")
                .SingleOrDefault();

            if (my_sql_client_row != null)
            {
                dataSet.Tables[0].Rows.Remove(my_sql_client_row);
            }

            var factory_type = is_merged ? "MySql.Data.MySqlClient.MySqlClientFactory, " + ApplicationParameters.get_merged_assembly_name(): "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data";

            dataSet.Tables[0].Rows.Add(
                "MySQL Data Provider",
                ".Net Framework Data Provider for MySQL",
                "MySql.Data.MySqlClient",
                factory_type);
        }

        public void enable_loading_from_merged_assembly()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => load_mysql_data_assembly(e.Name);
        }

        private Assembly load_mysql_data_assembly(string assembly_name)
        {
            if (is_merged && assembly_name == "MySql.Data")
            {
                return Assembly.GetExecutingAssembly();
            }

            return null;
        }
    }
}