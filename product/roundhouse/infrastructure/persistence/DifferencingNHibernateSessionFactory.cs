namespace roundhouse.infrastructure.persistence
{
    using System;
    using System.Reflection;
    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;
    using NHibernate;
    using NHibernate.Cfg;

    public class DifferencingNHibernateSessionFactory
    {
        private const string proxy_factory = NHibernate.Cfg.Environment.ProxyFactoryFactoryClass;
        private const string proxy_factory_name = "NHibernate.ByteCode.Castle.ProxyFactoryFactory";
        private static bool is_merged = true;
        
        public static ISessionFactory build_session_factory(string db_name, Assembly mappings_assembly,Assembly conventions_assembly,Action<Configuration> additional_function)
        {
            if (conventions_assembly == null) conventions_assembly = mappings_assembly;
            if (additional_function == null) additional_function = no_operation;

            return Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2008.ConnectionString(c => c.Server("(local)").Database(db_name).TrustedConnection()))
                    .Mappings(m =>
                        {
                            m.HbmMappings.AddFromAssembly(mappings_assembly);
                            m.FluentMappings.AddFromAssembly(mappings_assembly)
                                .Conventions.AddAssembly(conventions_assembly);
                        })
                .ExposeConfiguration(cfg => 
                                {
                                     
                                     string proxy_factory_location = proxy_factory_name + ", " + ApplicationParameters.get_merged_assembly_name();
                                     if (!is_merged) proxy_factory_location = proxy_factory_name + ", NHibernate.ByteCode.Castle";

                                     if (cfg.Properties.ContainsKey(proxy_factory))
                                     {
                                         
                                         cfg.Properties[proxy_factory] = proxy_factory_location;
                                     }
                                     else
                                     {
                                         cfg.Properties.Add(proxy_factory, proxy_factory_location);
                                     }
                                })
                .ExposeConfiguration(additional_function)
                .BuildSessionFactory();
        }

       
        private static void no_operation(Configuration cfg) { }

    }
}