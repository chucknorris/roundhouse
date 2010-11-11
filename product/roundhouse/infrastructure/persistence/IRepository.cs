namespace roundhouse.infrastructure.persistence
{
    using System.Collections.Generic;
    using NHibernate;
    using NHibernate.Cfg;
    using NHibernate.Criterion;

    public interface IRepository
    {
        IList<T> get_all<T>();
        IList<T> get_with_criteria<T>(DetachedCriteria detachedCriteria);
        IList<T> get_transformation_with_criteria<T>(DetachedCriteria detachedCriteria);
        void save_or_update<T>(IList<T> list);
        void save_or_update<T>(T item);
        void delete<T>(IList<T> list);

        ISessionFactory session_factory { get; }
        Configuration nhibernate_configuration { get; }
        //string connection_string { get; }
    }
}