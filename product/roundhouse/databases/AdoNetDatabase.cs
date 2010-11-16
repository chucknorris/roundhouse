namespace roundhouse.databases
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using connections;
    using parameters;

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

        public override void open_admin_connection()
        {
            server_connection = GetAdoNetConnection(admin_connection_string);
            server_connection.open();
        }

        public override void close_admin_connection()
        {
            server_connection.close();
        }

        public override void open_connection(bool with_transaction)
        {
            server_connection = GetAdoNetConnection(connection_string);
            server_connection.open();

            set_repository();

            if (with_transaction)
            {
                transaction = server_connection.underlying_type().BeginTransaction();
                repository.start(true);
            }
        }

        public override void close_connection()
        {
            if (transaction != null)
            {
                transaction.Commit();
                transaction = null;
            }

            repository.finish();

            server_connection.close();
        }

        public override void rollback() {
            repository.rollback();
 
            if (transaction != null)
            {
                //rollback previous transaction
                transaction.Rollback();
                server_connection.close();

                //open a new transaction
                server_connection.open();
                //use_database(database_name);
                transaction = server_connection.underlying_type().BeginTransaction();
                repository.start(true);
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

        protected IDbCommand setup_database_command(string sql_to_run, IEnumerable<IParameter<IDbDataParameter>> parameters)
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
    }
}