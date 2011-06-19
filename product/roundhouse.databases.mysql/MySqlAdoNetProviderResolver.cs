using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using roundhouse.infrastructure;
// ReSharper disable InconsistentNaming

namespace roundhouse.databases.mysql
{
	public class MySqlAdoNetProviderResolver
	{
		private readonly bool is_merged;

		public MySqlAdoNetProviderResolver()
		{
			is_merged = Assembly.GetExecutingAssembly().GetName().Name 
				== ApplicationParameters.get_merged_assembly_name();
		}

		public void register_db_provider_factory()
		{
			var dataSet = (DataSet)ConfigurationManager.GetSection("system.data");

			var mySqlClientRow = dataSet.Tables[0]
				.AsEnumerable()
				.Where(x => x.Field<string>(2) == "MySql.Data.MySqlClient")
				.SingleOrDefault();

			if (mySqlClientRow != null)
			{
				dataSet.Tables[0].Rows.Remove(mySqlClientRow);
			}

			var clientFactoryType = is_merged
				? "MySql.Data.MySqlClient.MySqlClientFactory, " + ApplicationParameters.get_merged_assembly_name()
				: "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data";

			dataSet.Tables[0].Rows.Add(
				"MySQL Data Provider",
				".Net Framework Data Provider for MySQL",
				"MySql.Data.MySqlClient",
				clientFactoryType);
		}

		public void enable_loading_from_merged_assembly()
		{
			AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => load_MySqlData_assembly(e.Name);
		}

		private Assembly load_MySqlData_assembly(string assemblyName)
		{
			if (is_merged && assemblyName == "MySql.Data")
			{
				return Assembly.GetExecutingAssembly();
			}

			return null;
		}
	}
}