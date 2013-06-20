using System;
using System.Data;
using System.Reflection;
using NHibernate.AdoNet;
using NHibernate.Driver;
using NHibernate.Engine.Query;
using NHibernate.SqlTypes;
using NHibernate.Util;
using Oracle.ManagedDataAccess.Client;

namespace roundhouse.databases.oracle
{
	public class RoundhousEOracleDriver : ReflectionBasedDriver, IEmbeddedBatcherFactoryProvider
	{
		private static readonly SqlType GuidSqlType = new SqlType(DbType.Binary, 16);
		private const string driverAssemblyName = "Oracle.ManagedDataAccess";
		private const string connectionTypeName = "Oracle.ManagedDataAccess.Client.OracleConnection";
		private const string commandTypeName = "Oracle.ManagedDataAccess.Client.OracleCommand";
		private readonly PropertyInfo oracleCommandBindByName;
		private readonly PropertyInfo oracleDbType;
		private readonly object oracleDbTypeRefCursor;

		/// <summary/>
		public override bool UseNamedPrefixInSql
		{
			get
			{
				return true;
			}
		}

		/// <summary/>
		public override bool UseNamedPrefixInParameter
		{
			get
			{
				return true;
			}
		}

		/// <summary/>
		public override string NamedPrefix
		{
			get
			{
				return ":";
			}
		}

		Type IEmbeddedBatcherFactoryProvider.BatcherFactoryClass
		{
			get
			{
				return typeof(OracleDataClientBatchingBatcherFactory);
			}
		}

		static RoundhousEOracleDriver()
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="T:NHibernate.Driver.OracleDataClientDriver"/>.
		/// 
		/// </summary>
		/// <exception cref="T:NHibernate.HibernateException">Thrown when the <c>Oracle.DataAccess</c> assembly can not be loaded.
		///             </exception>
		public RoundhousEOracleDriver()
			: base("Oracle.ManagedDataAccess.Client", "Oracle.ManagedDataAccess", "Oracle.ManagedDataAccess.Client.OracleConnection", "Oracle.ManagedDataAccess.Client.OracleCommand")
		{
			this.oracleCommandBindByName = ReflectHelper.TypeFromAssembly("Oracle.ManagedDataAccess.Client.OracleCommand", "Oracle.ManagedDataAccess", false).GetProperty("BindByName");
			this.oracleDbType = ReflectHelper.TypeFromAssembly("Oracle.ManagedDataAccess.Client.OracleParameter", "Oracle.ManagedDataAccess", false).GetProperty("OracleDbType");
			this.oracleDbTypeRefCursor = Enum.Parse(ReflectHelper.TypeFromAssembly("Oracle.ManagedDataAccess.Client.OracleDbType", "Oracle.ManagedDataAccess", false), "RefCursor");
		}

		/// <remarks>
		/// This adds logic to ensure that a DbType.Boolean parameter is not created since
		///             ODP.NET doesn't support it.
		/// 
		/// </remarks>
		protected override void InitializeParameter(IDbDataParameter dbParam, string name, SqlType sqlType)
		{
			switch (sqlType.DbType)
			{
				case DbType.Boolean:
					base.InitializeParameter(dbParam, name, SqlTypeFactory.Int16);
					break;
				case DbType.Guid:
					base.InitializeParameter(dbParam, name, GuidSqlType);
					break;
				default:
					base.InitializeParameter(dbParam, name, sqlType);
					break;
			}

			//http://thebasilet.blogspot.be/2009/07/nhibernate-oracle-clobs.html
			//MS OracleClient driver generates an exception
			//we set the IDbDataParameter.Value = (string whose length: 4000 > length > 2000 )
			//when we set the IDbDataParameter.DbType = DbType.String
			//when DB Column is of type NCLOB/CLOB
			//The Above is the default behavior for NHibernate.OracleClientDriver
			//So we use the built-in StringClobSqlType to tell the driver to use the NClob Oracle type
			//This will work for both NCLOB/CLOBs without issues.
			//Mapping file will need to be update to use StringClob as the property type
			if ((sqlType is StringClobSqlType))
			{
				((OracleParameter)dbParam).OracleDbType = OracleDbType.NClob;
			}
		}

		protected override void OnBeforePrepare(IDbCommand command)
		{
			base.OnBeforePrepare(command);
			this.oracleCommandBindByName.SetValue((object)command, (object)true, (object[])null);
			CallableParser.Detail detail = CallableParser.Parse(command.CommandText);
			if (!detail.IsCallable)
				return;
			command.CommandType = CommandType.StoredProcedure;
			command.CommandText = detail.FunctionName;
			this.oracleCommandBindByName.SetValue((object)command, (object)false, (object[])null);
			IDbDataParameter parameter = command.CreateParameter();
			this.oracleDbType.SetValue((object)parameter, this.oracleDbTypeRefCursor, (object[])null);
			parameter.Direction = detail.HasReturn ? ParameterDirection.ReturnValue : ParameterDirection.Output;
			command.Parameters.Insert(0, (object)parameter);
		}
	}
}
