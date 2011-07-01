namespace roundhouse.infrastructure.persistence
{
    using System;
    using System.Collections.Generic;
    using extensions;
    using logging;
    using NHibernate;
    using NHibernate.Cfg;
    using NHibernate.Criterion;
    using NHibernate.Transform;

    public sealed class Repository : IRepository
    {
        private bool running_in_a_transaction;

        public ISessionFactory session_factory { get; private set; }
        public Configuration nhibernate_configuration { get; private set; }
        public ITransaction transaction { get; private set; }
        private ISession session
        {
            get;
            set;
        }

        public Repository(ISessionFactory session_factory, Configuration cfg)
        {
            this.session_factory = session_factory;
            this.nhibernate_configuration = cfg;
            if (session_factory == null)
            {
                throw new ApplicationException("Repository cannot do any with a null session factory. Please provide a session factory.");
            }
        }

        public void start(bool using_transaction)
        {
            running_in_a_transaction = using_transaction;
            session = session_factory.OpenSession();
            if (using_transaction)
            {
                transaction = session.BeginTransaction();
            }
        }

        public void rollback()
        {
            if (running_in_a_transaction)
            {
                transaction.Rollback();
            }
            running_in_a_transaction = false;

            finish();
        }

        public void finish()
        {
            if (session != null && session.IsOpen)
            {
                if (running_in_a_transaction)
                {
                    transaction.Commit();
                }

                if (session == null) return;

                session.Close();
                session.Dispose();
                
            }
            session = null;
        }

        public IList<T> get_all<T>()
        {
            IList<T> list;
            Type persistentClass = typeof(T);

            bool not_running_outside_session = session == null;
            if (not_running_outside_session) start(false);

            ICriteria criteria = session.CreateCriteria(persistentClass);
            list = criteria.List<T>();

            if (not_running_outside_session) finish();

            Log.bound_to(this).log_a_debug_event_containing("Repository found {0} records of type {1}.", list.Count, typeof(T).Name);

            return list;
        }

        public IList<T> get_with_criteria<T>(DetachedCriteria detachedCriteria)
        {
            if (detachedCriteria == null)
            {
                Log.bound_to(this).log_a_warning_event_containing("Please ensure you send in a criteria when you want to limit records. Otherwise please consider using GetAll(). Returning empty list.");
                return null;
            }

            IList<T> list;

            bool not_running_outside_session = session == null;
            if (not_running_outside_session) start(false);

            ICriteria criteria = detachedCriteria.GetExecutableCriteria(session);
            list = criteria.List<T>();

            if (not_running_outside_session) finish();

            Log.bound_to(this).log_a_debug_event_containing("Repository found {0} records of type {1} with criteria {2}.", list.Count, typeof(T).Name, detachedCriteria.to_string());

            return list;
        }

        public IList<T> get_transformation_with_criteria<T>(DetachedCriteria detachedCriteria)
        {
            if (detachedCriteria == null)
            {
                Log.bound_to(this).log_a_warning_event_containing("Please ensure you send in a criteria when you want to get transformed records. Otherwise please consider using GetAll(). Returning empty list.");
                return null;
            }

            IList<T> list;

            bool running_long_session = session == null;
            if (!running_long_session) start(false);

            ICriteria criteria = detachedCriteria.GetExecutableCriteria(session);
            list = criteria
                .SetResultTransformer(new AliasToBeanResultTransformer(typeof(T)))
                .List<T>();

            if (!running_long_session) finish();

            Log.bound_to(this).log_a_debug_event_containing("Repository found {0} records of type {1} with criteria {2}.", list.Count, typeof(T).Name, detachedCriteria.to_string());

            return list;
        }

        public void save_or_update<T>(IList<T> list)
        {
            if (list == null || list.Count == 0)
            {
                Log.bound_to(this).log_a_warning_event_containing("Please ensure you send a non null list of records to save.");
                return;
            }
            Log.bound_to(this).log_a_debug_event_containing("Received {0} records of type {1} marked for save/update.", list.Count, typeof(T).Name);

            bool not_running_outside_session = session == null;
            if (not_running_outside_session) start(true);

            foreach (T item in list)
            {
                save_or_update(item);
            }

            if (not_running_outside_session) finish();

            Log.bound_to(this).log_a_debug_event_containing("Saved {0} records of type {1} successfully.", list.Count, typeof(T).Name);
        }

        public void save_or_update<T>(T item)
        {
            if (item == null)
            {
                Log.bound_to(this).log_a_warning_event_containing("Please ensure you send a non null record to save.");
                return;
            }

            bool not_running_outside_session = session == null;
            if (not_running_outside_session) start(false);

            session.SaveOrUpdate(item);
            session.Flush();

            if (not_running_outside_session) finish();

            Log.bound_to(this).log_a_debug_event_containing("Saved item of type {0} successfully.", typeof(T).Name);
        }

        public void delete<T>(IList<T> list)
        {
            if (list == null || list.Count == 0)
            {
                Log.bound_to(this).log_a_warning_event_containing("Please ensure you send a non null list of records to delete.");
                return;
            }

            Log.bound_to(this).log_an_info_event_containing("Received {0} records of type {1} marked for deletion.", list.Count, typeof(T).Name);

            bool not_running_outside_session = session == null;
            if (not_running_outside_session) start(true);

            foreach (T item in list)
            {
                session.Delete(item);
                session.Flush();
            }

            if (not_running_outside_session) finish();

            Log.bound_to(this).log_an_info_event_containing("Removed {0} records of type {1} successfully.", list.Count, typeof(T).Name);
        }

    }
}