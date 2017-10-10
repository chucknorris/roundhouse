using System.ComponentModel;
using System.Data.SqlClient;

namespace roundhouse.databases.sqlserver
{
    using System;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using infrastructure.logging;

    public class TransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        private readonly SqlDatabaseTransientErrorDetectionStrategy inner_strategy;

        public TransientErrorDetectionStrategy()
        {
            inner_strategy = new SqlDatabaseTransientErrorDetectionStrategy();
        }

        public bool IsTransient(Exception ex)
        {
            bool transient = IsTransientException(ex);
            if (ex != null)
            {
                Log.bound_to(this).log_a_debug_event_containing("Checking whether the '{0}: {1}' error is transient - {2} ", ex.GetType(), ex.Message, transient);
            }

            return transient;
        }

        private bool IsTransientException(Exception ex)
        {
            if (ex == null)
                return false;

            // Unwrap exception to handle exceptions wrapped by NHibernate GenericAdoException
            var inner = ex.InnerException;
            return inner_strategy.IsTransient(ex) || IsCustomTransientException(ex) || IsTransientException(inner);
        }

        private bool IsCustomTransientException(Exception ex)
        {
            var sql_exception = ex as SqlException;
            if (sql_exception == null)
            {
                return false;
            }

            // Borrowed from https://github.com/Azure/elastic-db-tools/blob/master/Src/ElasticScale.Client/ElasticScale.Common/TransientFaultHandling/Implementation/SqlDatabaseTransientErrorDetectionStrategy.cs#L167
            // Prelogin failure can happen due to waits expiring on windows handles. Or
            // due to a bug in the gateway code, a dropped database with a pooled connection
            // when reset results in a timeout error instead of immediate failure.
            Win32Exception wex = sql_exception.InnerException as Win32Exception;
            if (wex != null)
            {
                switch (wex.NativeErrorCode)
                {
                    // Timeout expired
                    case 0x102:
                        return true;

                    // Semaphore timeout expired
                    case 0x121:
                        return true;
                }
            }

            return false;
        }
    }
}