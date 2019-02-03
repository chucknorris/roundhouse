using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Xml;
using Polly;
using Polly.Retry;

/**
 * Ported to .NET Core from Enterprise Library - Transient Fault Handling Application Block:
 * Introduction: http://msdn.microsoft.com/entlib
 * Nuget: https://www.nuget.org/packages/EnterpriseLibrary.TransientFaultHandling/
 */

namespace roundhouse.databases.sqlserver
{
    internal sealed class ReliableSqlConnection : IDbConnection, ICloneable
    {
        private static readonly TransientErrorDetectionStrategy error_detection_strategy = new TransientErrorDetectionStrategy();
        
        private readonly SqlConnection underlying_connection;
        
        private static readonly RetryPolicy default_connection_retry_policy = get_default_retry_policy();
        private static readonly RetryPolicy default_command_retry_policy = get_default_retry_policy();
        
        private readonly RetryPolicy connection_retry_policy;
        private readonly RetryPolicy command_retry_policy;
        private readonly RetryPolicy connection_string_failover_policy = get_default_retry_policy();
        
        private string connection_string;
        
       
        private static RetryPolicy get_default_retry_policy()
        {
            return Policy
                .Handle<Exception>(ex => error_detection_strategy.is_transient(ex))
                .WaitAndRetry(new[]
                    {
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(5)
                    }
                );
        }
 
        

        /// <summary>
        /// Initializes a new instance of the <see cref="T:roundhouse.databases.sqlserver.ReliableSqlConnection" /> class with the specified connection string. Uses the default
        /// retry policy for connections and commands unless retry settings are provided in the connection string.
        /// </summary>
        /// <param name="connection_string">The connection string used to open the SQL Database.</param>
        public ReliableSqlConnection(string connection_string)
            : this(connection_string, default_connection_retry_policy)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:roundhouse.databases.sqlserver.ReliableSqlConnection" /> class with the specified connection string
        /// and a policy that defines whether to retry a request if a connection or command
        /// fails.
        /// </summary>
        /// <param name="connectionString">The connection string used to open the SQL Database.</param>
        /// <param name="retryPolicy">The retry policy that defines whether to retry a request if a connection or command fails.</param>
        public ReliableSqlConnection(string connectionString, RetryPolicy retryPolicy)
            : this(connectionString, retryPolicy, default_command_retry_policy ?? retryPolicy)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:roundhouse.databases.sqlserver.ReliableSqlConnection" /> class with the specified connection string
        /// and a policy that defines whether to retry a request if a connection or command
        /// fails.
        /// </summary>
        /// <param name="connectionString">The connection string used to open the SQL Database.</param>
        /// <param name="connectionRetryPolicy">The retry policy that defines whether to retry a request if a connection fails to be established.</param>
        /// <param name="commandRetryPolicy">The retry policy that defines whether to retry a request if a command fails to be executed.</param>
        public ReliableSqlConnection(string connectionString, RetryPolicy connectionRetryPolicy, RetryPolicy commandRetryPolicy)
        {
            this.connection_string = connectionString;
            underlying_connection = new SqlConnection(connectionString);
            this.connection_retry_policy = connectionRetryPolicy;
            this.command_retry_policy = commandRetryPolicy;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the connection string for opening a connection to the SQL Database.
        /// </summary>
        public string ConnectionString
        {
            get => connection_string;
            set
            {
                connection_string = value;
                underlying_connection.ConnectionString = value;
            }
        }

        /// <summary>
        /// Gets the policy that determines whether to retry a connection request, based on how many
        /// times the request has been made and the reason for the last failure.
        /// </summary>
        public RetryPolicy ConnectionRetryPolicy => connection_retry_policy;

        /// <summary>
        /// Gets the policy that determines whether to retry a command, based on how many
        /// times the request has been made and the reason for the last failure.
        /// </summary>
        public RetryPolicy CommandRetryPolicy => command_retry_policy;

        /// <summary>
        /// Gets an instance of the SqlConnection object that represents the connection to a SQL Database instance.
        /// </summary>
        public SqlConnection Current => underlying_connection;

       
        /// <summary>
        /// Gets a value that specifies the time to wait while trying to establish a connection before terminating
        /// the attempt and generating an error.
        /// </summary>
        public int ConnectionTimeout => underlying_connection.ConnectionTimeout;

        /// <summary>
        /// Gets the name of the current database or the database to be used after a
        /// connection is opened.
        /// </summary>
        public string Database => underlying_connection.Database;

        /// <summary>Gets the current state of the connection.</summary>
        public ConnectionState State => underlying_connection.State;

        /// <summary>
        /// Opens a database connection with the settings specified by the ConnectionString and ConnectionRetryPolicy properties.
        /// </summary>
        /// <returns>An object that represents the open connection.</returns>
        public SqlConnection Open()
        {
            return Open(ConnectionRetryPolicy);
        }

        /// <summary>
        /// Opens a database connection with the settings specified by the connection string and the specified retry policy.
        /// </summary>
        /// <param name="retryPolicy">The retry policy that defines whether to retry a request if the connection fails to open.</param>
        /// <returns>An object that represents the open connection.</returns>
        public SqlConnection Open(RetryPolicy retryPolicy)
        {
            retryPolicy.Execute(() =>
                {
                    if (underlying_connection.State == ConnectionState.Open)
                        return;
                    underlying_connection.Open();
                });
            return underlying_connection;
        }

        /// <summary>
        /// Executes a SQL command and returns a result that is defined by the specified type <typeparamref name="T" />. This method uses the retry policy specified when
        /// instantiating the SqlAzureConnection class (or the default retry policy if no policy was set at construction time).
        /// </summary>
        /// <typeparam name="T">IDataReader, XmlReader, or any other .NET Framework type that defines the type of result to be returned.</typeparam>
        /// <param name="command">The SQL command to be executed.</param>
        /// <returns>An instance of an IDataReader, XmlReader, or any other .NET Framework object that contains the result.</returns>
        public T ExecuteCommand<T>(IDbCommand command)
        {
            return ExecuteCommand<T>(command, CommandRetryPolicy, CommandBehavior.Default);
        }

        /// <summary>
        /// Executes a SQL command and returns a result that is defined by the specified type <typeparamref name="T" />. This method uses the retry policy specified when
        /// instantiating the SqlAzureConnection class (or the default retry policy if no policy was set at construction time).
        /// </summary>
        /// <typeparam name="T">IDataReader, XmlReader, or any other .NET Framework type that defines the type of result to be returned.</typeparam>
        /// <param name="command">The SQL command to be executed.</param>
        /// <param name="behavior">A description of the results of the query and its effect on the database.</param>
        /// <returns>An instance of an IDataReader, XmlReader, or any other .NET Frameork object that contains the result.</returns>
        public T ExecuteCommand<T>(IDbCommand command, CommandBehavior behavior)
        {
            return ExecuteCommand<T>(command, CommandRetryPolicy, behavior);
        }

        /// <summary>
        /// Executes a SQL command by using the specified retry policy, and returns a result that is defined by the specified type <typeparamref name="T" />
        /// </summary>
        /// <typeparam name="T">IDataReader, XmlReader, or any other .NET Framework type that defines the type of result to be returned.</typeparam>
        /// <param name="command">The SQL command to be executed.</param>
        /// <param name="retryPolicy">The retry policy that defines whether to retry a command if a connection fails while executing the command.</param>
        /// <returns>An instance of an IDataReader, XmlReader, or any other .NET Frameork object that contains the result.</returns>
        public T ExecuteCommand<T>(IDbCommand command, RetryPolicy retryPolicy)
        {
            return ExecuteCommand<T>(command, retryPolicy, CommandBehavior.Default);
        }

        /// <summary>
        /// Executes a SQL command by using the specified retry policy, and returns a result that is defined by the specified type <typeparamref name="T" />
        /// </summary>
        /// <typeparam name="T">IDataReader, XmlReader, or any other .NET Framework type that defines the type of result to be returned.</typeparam>
        /// <param name="command">The SQL command to be executed.</param>
        /// <param name="retry_policy">The retry policy that defines whether to retry a command if a connection fails while executing the command.</param>
        /// <param name="behavior">A description of the results of the query and its effect on the database.</param>
        /// <returns>An instance of an IDataReader, XmlReader, or any other .NET Frameork object that contains the result.</returns>
        public T ExecuteCommand<T>(IDbCommand command, RetryPolicy retry_policy, CommandBehavior behavior)
        {
            T action_result = default(T);
            Type result_type = typeof(T);
            bool has_opened_connection = false;
            bool close_opened_connection_on_success = false;
            try
            {
                retry_policy.Execute((Action) (() => action_result =
                    connection_string_failover_policy.Execute<T>( (() =>
                    {
                        if (command.Connection == null)
                        {
                            command.Connection = Open();
                            has_opened_connection = true;
                        }

                        if (command.Connection.State != ConnectionState.Open)
                        {
                            command.Connection.Open();
                            has_opened_connection = true;
                        }

                        if (typeof(IDataReader).IsAssignableFrom(result_type))
                        {
                            close_opened_connection_on_success = false;
                            return (T) command.ExecuteReader(behavior);
                        }

                        if (result_type == typeof(XmlReader))
                        {
                            if (!(command is SqlCommand))
                                throw new NotSupportedException();
                            XmlReader inner_reader = (command as SqlCommand).ExecuteXmlReader();
                            close_opened_connection_on_success = false;
                            if ((behavior & CommandBehavior.CloseConnection) != CommandBehavior.CloseConnection)
                                return (T) (object) inner_reader;
                            else
                                throw new ApplicationException("Don't have any SqlXmlReader");
                        }

                        if (result_type == typeof(NonQueryResult))
                        {
                            NonQueryResult nonQueryResult =
                                new NonQueryResult()
                                {
                                    RecordsAffected = command.ExecuteNonQuery()
                                };
                            close_opened_connection_on_success = true;
                            return (T) Convert.ChangeType(nonQueryResult, result_type,
                                CultureInfo.InvariantCulture);
                        }

                        object obj = command.ExecuteScalar();
                        close_opened_connection_on_success = true;
                        if (obj != null)
                            return (T) Convert.ChangeType(obj, result_type,
                                CultureInfo.InvariantCulture);
                        return default(T);
                    }))));
                if (has_opened_connection)
                {
                    if (close_opened_connection_on_success)
                    {
                        if (command.Connection != null)
                        {
                            if (command.Connection.State == ConnectionState.Open)
                                command.Connection.Close();
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (has_opened_connection && command.Connection != null &&
                    command.Connection.State == ConnectionState.Open)
                    command.Connection.Close();
                throw;
            }

            return action_result;
        }

        /// <summary>
        /// Executes a SQL command and returns the number of rows affected.
        /// </summary>
        /// <param name="command">The SQL command to be executed.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteCommand(IDbCommand command)
        {
            return ExecuteCommand(command, CommandRetryPolicy);
        }

        /// <summary>
        /// Executes a SQL command and returns the number of rows affected.
        /// </summary>
        /// <param name="command">The SQL command to be executed.</param>
        /// <param name="retryPolicy">The retry policy that defines whether to retry a command if a connection fails while executing the command.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteCommand(IDbCommand command, RetryPolicy retryPolicy)
        {
            return ExecuteCommand<NonQueryResult>(command, retryPolicy).RecordsAffected;
        }

        /// <summary>
        /// Begins a database transaction with the specified System.Data.IsolationLevel value.
        /// </summary>
        /// <param name="il">One of the enumeration values that specifies the isolation level for the transaction.</param>
        /// <returns>An object that represents the new transaction.</returns>
        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return underlying_connection.BeginTransaction(il);
        }

        /// <summary>Begins a database transaction.</summary>
        /// <returns>An object that represents the new transaction.</returns>
        public IDbTransaction BeginTransaction()
        {
            return underlying_connection.BeginTransaction();
        }

        /// <summary>
        /// Changes the current database for an open Connection object.
        /// </summary>
        /// <param name="databaseName">The name of the database to use in place of the current database.</param>
        public void ChangeDatabase(string databaseName)
        {
            underlying_connection.ChangeDatabase(databaseName);
        }

        void IDbConnection.Open()
        {
            this.Open();
        }

        /// <summary>Closes the connection to the database.</summary>
        public void Close()
        {
            underlying_connection.Close();
        }

        /// <summary>
        /// Creates and returns a SqlCommand object that is associated with the underlying SqlConnection.
        /// </summary>
        /// <returns>A System.Data.SqlClient.SqlCommand object that is associated with the underlying connection.</returns>
        public SqlCommand CreateCommand()
        {
            return underlying_connection.CreateCommand();
        }

        IDbCommand IDbConnection.CreateCommand()
        {
            return underlying_connection.CreateCommand();
        }

        object ICloneable.Clone()
        {
            return new ReliableSqlConnection(ConnectionString, ConnectionRetryPolicy,
                CommandRetryPolicy);
        }

        /// <summary>
        /// Performs application-defined tasks that are associated with freeing, releasing, or
        /// resetting managed and unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">A flag indicating that managed resources must be released.</param>
        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            if (underlying_connection.State == ConnectionState.Open)
                underlying_connection.Close();
            underlying_connection.Dispose();
        }

        /// <summary>
        /// This helpers class is intended to be used exclusively for fetching the number of affected records when executing a command by using ExecuteNonQuery.
        /// </summary>
        private sealed class NonQueryResult
        {
            public int RecordsAffected { get; set; }
        }
    }
}