using System;
using System.Data;
using System.Linq;
using System.Reflection;

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
            try
            {
                server_connection.Open();
            }catch (ReflectionTypeLoadException rtle)
            {
                throw rtle.LoaderExceptions.First();
            }

            catch (TypeInitializationException tie)
            {
                var inner = tie.InnerException;
                if (inner is ReflectionTypeLoadException r)
                {
                    throw r.LoaderExceptions.First();
                }
            }
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
            if (server_connection != null && server_connection.State != ConnectionState.Closed)
            {
                server_connection.Close();
            }
        }

        public IDbConnection underlying_type()
        {
            return server_connection;
        }

        void IDisposable.Dispose()
        {
            server_connection.Dispose();
        }
    }
}