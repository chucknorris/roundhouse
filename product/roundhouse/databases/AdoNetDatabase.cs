using System;
using roundhouse.infrastructure.app;
using roundhouse.infrastructure.logging;

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
            Log.bound_to(this).log_a_debug_event_containing("Opening admin connection to '{0}'", admin_connection_string);
            admin_connection = GetAdoNetConnection(admin_connection_string);
            admin_connection.open();
        }

        public override void close_admin_connection()
        {
            Log.bound_to(this).log_a_debug_event_containing("Closing admin connection");
            admin_connection.close();
        }

        public override void open_connection(bool with_transaction)
        {
            Log.bound_to(this).log_a_debug_event_containing("Opening connection to '{0}'", connection_string);
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
            Log.bound_to(this).log_a_debug_event_containing("Closing connection");
            if (transaction != null)
            {
                transaction.Commit();
                transaction = null;
            }
            if (repository != null)
            {
                repository.finish();    
            }
            
            if (server_connection != null)
            {
                server_connection.close();    
            }
        }

        public override void rollback()
        {
            Log.bound_to(this).log_a_debug_event_containing("Rolling back changes");
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

        protected override void run_sql(string sql_to_run, ConnectionType connection_type, IList<IParameter<IDbDataParameter>> parameters)
        {
            if (string.IsNullOrEmpty(sql_to_run)) return;

            //really naive retry logic. Consider Lokad retry policy
            //this is due to sql server holding onto a connection http://social.msdn.microsoft.com/Forums/en-US/adodotnetdataproviders/thread/99963999-a59b-4614-a1b9-869c6dff921e
            try
            {
                run_command_with(sql_to_run, connection_type, parameters);
            }
            catch (Exception ex)
            {
                Log.bound_to(this).log_a_debug_event_containing("Failure executing command, trying again. {0}{1}",Environment.NewLine,ex.ToString());
                run_command_with(sql_to_run, connection_type, parameters);
            }
        }

        private void run_command_with(string sql_to_run,ConnectionType connection_type, IList<IParameter<IDbDataParameter>> parameters)
        {
             using (IDbCommand command = setup_database_command(sql_to_run, connection_type, parameters))
            {
                command.ExecuteNonQuery();
                command.Dispose();
            }
        }

        protected override object run_sql_scalar(string sql_to_run, ConnectionType connection_type, IList<IParameter<IDbDataParameter>> parameters)
        {
            object return_value = new object();
            if (string.IsNullOrEmpty(sql_to_run)) return return_value;

            using (IDbCommand command = setup_database_command(sql_to_run, connection_type, null))
            {
                return_value = command.ExecuteScalar();
                command.Dispose();
            }

            return return_value;
        }

        protected IDbCommand setup_database_command(string sql_to_run, ConnectionType connection_type, IEnumerable<IParameter<IDbDataParameter>> parameters)
        {
            IDbCommand command = null;
            switch (connection_type)
            {
                case ConnectionType.Default:
                    if (server_connection == null || server_connection.underlying_type().State != ConnectionState.Open)
                    {
                        open_connection(false);
                    }
                    Log.bound_to(this).log_a_debug_event_containing("Setting up command for normal connection");
                    command = server_connection.underlying_type().CreateCommand();
                    command.CommandTimeout = command_timeout;
                    break;
                case ConnectionType.Admin:
                    if (admin_connection == null || admin_connection.underlying_type().State != ConnectionState.Open)
                    {
                        open_admin_connection();
                    }
                    Log.bound_to(this).log_a_debug_event_containing("Setting up command for admin connection");
                    command = admin_connection.underlying_type().CreateCommand();
                    command.CommandTimeout = admin_command_timeout;
                    break;
            }

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

            return command;
        }
    }
}