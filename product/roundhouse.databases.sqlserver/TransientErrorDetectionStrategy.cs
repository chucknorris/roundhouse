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
            return inner_strategy.IsTransient(ex) || IsTransientException(ex.InnerException);
        }
    }
}