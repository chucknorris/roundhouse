using System.Collections.Generic;
using System.Data.Common;
using roundhouse.connections;
using roundhouse.parameters;

namespace roundhouse.databases
{
    using System.Data;
    using sql;

    public abstract class AdoNetDatabase : DefaultDatabase<IDbConnection>
    {
        private bool split_batches_in_ado = true;
        public override bool split_batch_statements
        {
            get { return split_batches_in_ado; }
            set { split_batches_in_ado = value; }
        }
        protected IDbTransaction transaction;

        private DbProviderFactory provider_factory;
        
        private AdoNetConnection GetAdoNetConnection(string conn_string)
        {
            provider_factory = DbProviderFactories.GetFactory(provider);
            IDbConnection connection = provider_factory.CreateConnection();
            connection.ConnectionString = conn_string;
            return new AdoNetConnection(connection);
        }

        public override void set_provider_and_sql_scripts()
        {
            provider = "System.Data.SqlClient";
            SqlScripts.sql_scripts_dictionary.TryGetValue(provider, out sql_scripts);
            if (sql_scripts == null)
            {
                sql_scripts = SqlScripts.t_sql_scripts;
            }
        }

        public override void open_connection(bool with_transaction)
        {
            server_connection = GetAdoNetConnection(connection_string);
            server_connection.open();

            if (with_transaction)
            {
                transaction = server_connection.underlying_type().BeginTransaction();
            }
        }

        public override void close_connection()
        {
            if (transaction != null)
            {
                transaction.Commit();
                transaction = null;
            }

            server_connection.close();
        }

        public override void open_admin_connection()
        {
            server_connection = GetAdoNetConnection(admin_connection_string);
            server_connection.open();
        }

        public override void close_admin_connection()
        {
            server_connection.close();
        }

        public override void rollback()
        {
            if (transaction != null)
            {
                //rollback previous transaction
                transaction.Rollback();
                server_connection.close();

                //open a new transaction
                server_connection.open();
                use_database(database_name);
                transaction = server_connection.underlying_type().BeginTransaction();
            }
        }

        protected override void run_sql(string sql_to_run, IList<IParameter<IDbDataParameter>> parameters)
        {
            if (string.IsNullOrEmpty(sql_to_run)) return;

            using (IDbCommand command = setup_database_command(sql_to_run, parameters))
            {
                command.ExecuteNonQuery();
                command.Dispose();
            }
        }

        protected override object run_sql_scalar(string sql_to_run, IList<IParameter<IDbDataParameter>> parameters)
        {
            object return_value = new object();

            if (string.IsNullOrEmpty(sql_to_run)) return return_value;

            using (IDbCommand command = setup_database_command(sql_to_run, parameters))
            {
                return_value = command.ExecuteScalar();
                command.Dispose();
            }

            return return_value;
        }

        protected override DataTable execute_datatable(string sql_to_run, IEnumerable<IParameter<IDbDataParameter>> parameters)
        {
            DataSet result = new DataSet();

            using (IDbCommand command = setup_database_command(sql_to_run, parameters))
            {
                using (IDataReader data_reader = command.ExecuteReader())
                {
                    DataTable data_table = new DataTable();
                    data_table.Load(data_reader);
                    data_reader.Close();
                    data_reader.Dispose();

                    result.Tables.Add(data_table);
                }
                command.Dispose();
            }

            return result.Tables.Count == 0 ? null : result.Tables[0];
        }

        private IDbCommand setup_database_command(string sql_to_run, IEnumerable<IParameter<IDbDataParameter>> parameters)
        {
            IDbCommand command = server_connection.underlying_type().CreateCommand();
            if (parameters != null)
            {
                foreach (IParameter<IDbDataParameter> parameter in parameters)
                {
                    command.Parameters.Add(parameter.underlying_type);
                }
            }
            command.Transaction = transaction;
            command.CommandText = sql_to_run;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = command_timeout;

            return command;
        }

        protected override IParameter<IDbDataParameter> create_parameter(string name, DbType type, object value, int? size)
        {
            IDbCommand command = server_connection.underlying_type().CreateCommand();
            var parameter = command.CreateParameter();
            command.Dispose();

            parameter.Direction = ParameterDirection.Input;
            parameter.ParameterName = name;
            parameter.DbType = type;
            parameter.Value = value;
            if (size != null)
            {
                parameter.Size = size.Value;
            }

            return new AdoNetParameter(parameter);
        }

        
    }
}