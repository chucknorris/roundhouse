using System.ComponentModel;
using System.Data.SqlClient;

namespace roundhouse.databases.sqlserver
{
    using System;
    using infrastructure.logging;

    public class TransientErrorDetectionStrategy 
    {
        public bool is_transient(Exception ex)
        {
            bool transient = is_transient_exception(ex);
            if (ex != null)
            {
                Log.bound_to(this).log_a_debug_event_containing("Checking whether the '{0}: {1}' error is transient - {2} ", ex.GetType(), ex.Message, transient);
            }

            return transient;
        }

        private static bool is_transient_exception(Exception ex)
        {
            if (ex == null)
                return false;

            // Unwrap exception to handle exceptions wrapped by NHibernate GenericAdoException
            var inner = ex.InnerException;
            return is_transient_core(ex) || is_custom_transient_exception(ex) || is_transient_exception(inner);
        }

        private static bool is_custom_transient_exception(Exception ex)
        {
            var sql_exception = ex as SqlException;

            // Borrowed from https://github.com/Azure/elastic-db-tools/blob/master/Src/ElasticScale.Client/ElasticScale.Common/TransientFaultHandling/Implementation/SqlDatabaseTransientErrorDetectionStrategy.cs#L167
            // Prelogin failure can happen due to waits expiring on windows handles. Or
            // due to a bug in the gateway code, a dropped database with a pooled connection
            // when reset results in a timeout error instead of immediate failure.
            if (sql_exception?.InnerException is Win32Exception wex)
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

        private static bool is_transient_core(Exception ex)
        {
            if (ex != null)
            {
                SqlException sql_exception;
                if ((sql_exception = ex as SqlException) != null)
                {
                    foreach (SqlError error in sql_exception.Errors)
                    {
                        switch (error.Number)
                        {
                            case 20:
                            case 64:
                            case 233:
                            case 10053:
                            case 10054:
                            case 10060:
                            case 10928:
                            case 10929:
                            case 40143:
                            case 40197:
                            case 40540:
                            case 40613:
                                return true;
                            case 40501:
                                return true;
                            default:
                                continue;
                        }
                    }
                }
                else
                {
                    if (ex is TimeoutException)
                        return true;
                }
            }
            return false;
        }
    }
}