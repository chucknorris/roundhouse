// ReSharper disable InconsistentNaming

using System.Data.Common;
using MySql.Data.MySqlClient;

namespace roundhouse.databases.mysql
{
	using System;
    using infrastructure.app;
    using infrastructure.extensions;
    using infrastructure.logging;

	public class MySqlDatabase : AdoNetDatabase
	{
		public override void initialize_connections(ConfigurationPropertyHolder configuration_property_holder)
		{
			// IMO in this method SqlServerDatabase and OracleDatabase implementations has unmaintainable code,
			// I'm not sure that DbConnectionStringBuilder could be useful here but I will just skip all parsing code.
			// The main goal of pasing is to get full connection string and put it into configuration_property_holder.ConnectionString.

			configuration_property_holder.ConnectionString = connection_string;

			set_provider();
			if (string.IsNullOrEmpty(admin_connection_string))
			{
				var builder = new MySqlConnectionStringBuilder(connection_string);
				builder.Database = "master";
				admin_connection_string = builder.ConnectionString;
			}

			// TODO: Initialize configuration/system.data/DbProviderFactories app.config section in code.
		}

		public override void set_provider()
		{
			// TODO Investigate how to get provider name using MySQL API instead of hardcoded string.
			// http://stackoverflow.com/questions/1216626/how-to-use-ado-net-dbproviderfactory-with-mysql/1216887#1216887
			provider = "MySql.Data.MySqlClient";
		}

		public override string create_database_script()
		{
			throw new NotImplementedException();
		}

		public override string set_recovery_mode_script(bool simple)
		{
			throw new NotImplementedException();
		}

		public override string restore_database_script(string restore_from_path, string custom_restore_options)
		{
			throw new NotImplementedException();
		}

		public override string delete_database_script()
		{
			throw new NotImplementedException();
		}

		public override void run_database_specific_tasks()
		{
			throw new NotImplementedException();
		}
	}
}