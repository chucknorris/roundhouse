using MySql.Data.MySqlClient;
// ReSharper disable InconsistentNaming

namespace roundhouse.databases.mysql
{
	using System;
    using infrastructure.app;
    using infrastructure.extensions;

	public class MySqlDatabase : AdoNetDatabase
	{
		private readonly MySqlAdoNetProviderResolver _mySqlAdoNetProviderResolver;

		public MySqlDatabase()
		{
			_mySqlAdoNetProviderResolver = new MySqlAdoNetProviderResolver();
			_mySqlAdoNetProviderResolver.register_db_provider_factory();
			_mySqlAdoNetProviderResolver.enable_loading_from_merged_assembly();
		}

		public override bool split_batch_statements
		{
			get { return false; }
			set { throw new Exception("This options could not be changed because MySQL database migrator always splits batch statements by using MySqlScript class from MySQL ADO.NET provider"); }
		}

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
				builder.Database = "information_schema";
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
			return string.Format(
				@"CREATE DATABASE IF NOT EXISTS `{0}`;",
				database_name);
		}

		public override void run_database_specific_tasks()
		{
			// TODO Do we need anything for MySQL? For ex MS SQL Server implemenation has schema creation script and Oracle creates db-sequences.
			// TODO MS SQL Server could support different database schema for migration tables but MySQL doesn't support this actually.
			// For MySQL CREATE SCHEMA just alias to CREATE DATABASE
		}

		// http://bugs.mysql.com/bug.php?id=46429
		public override void run_sql(string sql_to_run)
		{
			if (string.IsNullOrEmpty(sql_to_run)) return;

			// TODO Investigate how pass CommandTimeout into commands which will be during MySqlScript execution.
			var connection = server_connection.underlying_type().downcast_to<MySqlConnection>();
			var script = new MySqlScript(connection, sql_to_run);
			script.Execute();
		}

		public override string set_recovery_mode_script(bool simple)
		{
			// TODO Do MySQL has recovery modes which could be initialized by running SQL statement?
			return string.Empty;
		}

		public override string restore_database_script(string restore_from_path, string custom_restore_options)
		{
			throw new NotImplementedException();
		}

		public override string delete_database_script()
		{
			throw new NotImplementedException();
		}
	}
}