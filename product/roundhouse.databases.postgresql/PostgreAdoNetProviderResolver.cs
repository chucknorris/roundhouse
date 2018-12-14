﻿namespace roundhouse.databases.postgresql
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using infrastructure;

    public class PostgreAdoNetProviderResolver
    {
        private readonly bool is_merged;

        public PostgreAdoNetProviderResolver()
        {
            is_merged = Assembly.GetExecutingAssembly().GetName().Name == ApplicationParameters.get_merged_assembly_name();
        }
 
        public void register_db_provider_factory()
        {
            var data_set = (DataSet) ConfigurationManager.GetSection("system.data");

            if (data_set != null)
            {
                var my_sql_client_row = data_set?.Tables[0]
                    .Rows?.Cast<DataRow>()
                    .SingleOrDefault(row => (string) row[2] == "Npgsql");

                if (my_sql_client_row != null) data_set.Tables[0].Rows.Remove(my_sql_client_row);

                var factory_type = is_merged ? "Npgsql.NpgsqlFactory, " + ApplicationParameters.get_merged_assembly_name() : "Npgsql.NpgsqlFactory, Npgsql";


                data_set.Tables[0].Rows.Add(
                    "PostgreSQL Data Provider",
                    ".Net Framework Data Provider for PostgreSQL",
                    "Npgsql",
                    factory_type);
            }
        }


        public void enable_loading_from_merged_assembly()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => load_mysql_data_assembly(e.Name);
        }

        private Assembly load_mysql_data_assembly(string assembly_name)
        {
            if (is_merged && assembly_name == "Npgsql")
            {
                return Assembly.GetExecutingAssembly();
            }

            return null;
        }
    }
}