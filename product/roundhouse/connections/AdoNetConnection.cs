using System.Data;

namespace roundhouse.connections
{
    using System.Data.SqlClient;

    public class AdoNetConnection : IConnection<IDbConnection>
    {
        private readonly IDbConnection server_connection;

        public AdoNetConnection(IDbConnection server_connection)
        {
            this.server_connection = server_connection;
        }

        public void open()
        {
            server_connection.Open();
        }

        public void clear_pool()
        {
            var sql_conn = server_connection as SqlConnection;
            if (sql_conn != null)
            {
                SqlConnection.ClearPool(sql_conn);
            }
        }

        public void close()
        {
            if (server_connection !=null && server_connection.State != ConnectionState.Closed)
            {
                server_connection.Close();    
            }
        }

        public IDbConnection underlying_type()
        {
            return server_connection;
        }

        public void Dispose()
        {
            server_connection.Dispose();
        }
    }
}