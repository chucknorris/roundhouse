using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Xml;
using Polly;
using Polly.Retry;

namespace roundhouse.databases.sqlserver
{
    public sealed class ReliableSqlConnection : IDbConnection, IDisposable, ICloneable
    {
        private readonly static TransientErrorDetectionStrategy error_detection_strategy = new TransientErrorDetectionStrategy();
        
        private readonly SqlConnection underlyingConnection;
        private readonly RetryPolicy connectionRetryPolicy = GetDefaultRetryPolicy();

        private readonly RetryPolicy commandRetryPolicy = GetDefaultRetryPolicy();
        private readonly RetryPolicy connectionStringFailoverPolicy = GetDefaultRetryPolicy();
        private string connectionString;
        private static RetryPolicy defaultConnectionRetryPolicy = GetDefaultRetryPolicy();
        private static RetryPolicy defaultCommandRetryPolicy = GetDefaultRetryPolicy();
        
       
        private static RetryPolicy GetDefaultRetryPolicy()
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
        /// <param name="connectionString">The connection string used to open the SQL Database.</param>
        public ReliableSqlConnection(string connectionString)
            : this(connectionString, defaultConnectionRetryPolicy)
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
            : this(connectionString, retryPolicy, defaultCommandRetryPolicy ?? retryPolicy)
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
            this.connectionString = connectionString;
            underlyingConnection = new SqlConnection(connectionString);
            this.connectionRetryPolicy = connectionRetryPolicy;
            this.commandRetryPolicy = commandRetryPolicy;
        }

        /// <summary>
        /// Gets or sets the connection string for opening a connection to the SQL Database.
        /// </summary>
        public string ConnectionString
        {
            get => connectionString;
            set
            {
                connectionString = value;
                underlyingConnection.ConnectionString = value;
            }
        }

        /// <summary>
        /// Gets the policy that determines whether to retry a connection request, based on how many
        /// times the request has been made and the reason for the last failure.
        /// </summary>
        public RetryPolicy ConnectionRetryPolicy => connectionRetryPolicy;

        /// <summary>
        /// Gets the policy that determines whether to retry a command, based on how many
        /// times the request has been made and the reason for the last failure.
        /// </summary>
        public RetryPolicy CommandRetryPolicy => commandRetryPolicy;

        /// <summary>
        /// Gets an instance of the SqlConnection object that represents the connection to a SQL Database instance.
        /// </summary>
        public SqlConnection Current => underlyingConnection;

       
        /// <summary>
        /// Gets a value that specifies the time to wait while trying to establish a connection before terminating
        /// the attempt and generating an error.
        /// </summary>
        public int ConnectionTimeout => underlyingConnection.ConnectionTimeout;

        /// <summary>
        /// Gets the name of the current database or the database to be used after a
        /// connection is opened.
        /// </summary>
        public string Database => underlyingConnection.Database;

        /// <summary>Gets the current state of the connection.</summary>
        public ConnectionState State => underlyingConnection.State;

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
                    if (underlyingConnection.State == ConnectionState.Open)
                        return;
                    underlyingConnection.Open();
                });
            return underlyingConnection;
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
        /// <param name="retryPolicy">The retry policy that defines whether to retry a command if a connection fails while executing the command.</param>
        /// <param name="behavior">A description of the results of the query and its effect on the database.</param>
        /// <returns>An instance of an IDataReader, XmlReader, or any other .NET Frameork object that contains the result.</returns>
        public T ExecuteCommand<T>(IDbCommand command, RetryPolicy retryPolicy, CommandBehavior behavior)
        {
            T actionResult = default(T);
            Type resultType = typeof(T);
            bool hasOpenedConnection = false;
            bool closeOpenedConnectionOnSuccess = false;
            try
            {
                retryPolicy.Execute((Action) (() => actionResult =
                    connectionStringFailoverPolicy.Execute<T>( (() =>
                    {
                        if (command.Connection == null)
                        {
                            command.Connection = Open();
                            hasOpenedConnection = true;
                        }

                        if (command.Connection.State != ConnectionState.Open)
                        {
                            command.Connection.Open();
                            hasOpenedConnection = true;
                        }

                        if (typeof(IDataReader).IsAssignableFrom(resultType))
                        {
                            closeOpenedConnectionOnSuccess = false;
                            return (T) command.ExecuteReader(behavior);
                        }

                        if (resultType == typeof(XmlReader))
                        {
                            if (!(command is SqlCommand))
                                throw new NotSupportedException();
                            XmlReader innerReader = (command as SqlCommand).ExecuteXmlReader();
                            closeOpenedConnectionOnSuccess = false;
                            if ((behavior & CommandBehavior.CloseConnection) != CommandBehavior.CloseConnection)
                                return (T) (object) innerReader;
                            else
                                throw new ApplicationException("Don't have any SqlXmlReader");
                        }

                        if (resultType == typeof(NonQueryResult))
                        {
                            NonQueryResult nonQueryResult =
                                new NonQueryResult()
                                {
                                    RecordsAffected = command.ExecuteNonQuery()
                                };
                            closeOpenedConnectionOnSuccess = true;
                            return (T) Convert.ChangeType(nonQueryResult, resultType,
                                CultureInfo.InvariantCulture);
                        }

                        object obj = command.ExecuteScalar();
                        closeOpenedConnectionOnSuccess = true;
                        if (obj != null)
                            return (T) Convert.ChangeType(obj, resultType,
                                CultureInfo.InvariantCulture);
                        return default(T);
                    }))));
                if (hasOpenedConnection)
                {
                    if (closeOpenedConnectionOnSuccess)
                    {
                        if (command.Connection != null)
                        {
                            if (command.Connection.State == ConnectionState.Open)
                                command.Connection.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (hasOpenedConnection && command.Connection != null &&
                    command.Connection.State == ConnectionState.Open)
                    command.Connection.Close();
                throw;
            }

            return actionResult;
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
            return underlyingConnection.BeginTransaction(il);
        }

        /// <summary>Begins a database transaction.</summary>
        /// <returns>An object that represents the new transaction.</returns>
        public IDbTransaction BeginTransaction()
        {
            return underlyingConnection.BeginTransaction();
        }

        /// <summary>
        /// Changes the current database for an open Connection object.
        /// </summary>
        /// <param name="databaseName">The name of the database to use in place of the current database.</param>
        public void ChangeDatabase(string databaseName)
        {
            underlyingConnection.ChangeDatabase(databaseName);
        }

        void IDbConnection.Open()
        {
            this.Open();
        }

        /// <summary>Closes the connection to the database.</summary>
        public void Close()
        {
            underlyingConnection.Close();
        }

        /// <summary>
        /// Creates and returns a SqlCommand object that is associated with the underlying SqlConnection.
        /// </summary>
        /// <returns>A System.Data.SqlClient.SqlCommand object that is associated with the underlying connection.</returns>
        public SqlCommand CreateCommand()
        {
            return underlyingConnection.CreateCommand();
        }

        IDbCommand IDbConnection.CreateCommand()
        {
            return underlyingConnection.CreateCommand();
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
            if (underlyingConnection.State == ConnectionState.Open)
                underlyingConnection.Close();
            underlyingConnection.Dispose();
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