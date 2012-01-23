//namespace roundhouse.infrastructure.persistence
//{
//    using System;
//    using System.Reflection;
//    using FluentNHibernate.Cfg;
//    using FluentNHibernate.Cfg.Db;
//    using NHibernate;
//    using NHibernate.Cfg;
//    using System.Linq;

//    public class DifferencingNHibernateSessionFactory
//    {
//        private const string proxy_factory = NHibernate.Cfg.Environment.ProxyFactoryFactoryClass;
//        private static bool is_merged = true;

//        public static ISessionFactory build_session_factory(string db_name, Assembly mappings_assembly, Assembly conventions_assembly, Action<Configuration> additional_function)
//        {
//            if (conventions_assembly == null) conventions_assembly = mappings_assembly;
//            if (additional_function == null) additional_function = no_operation;

//            return Fluently.Configure()
//                .Database(MsSqlConfiguration.MsSql2008.ConnectionString(c => c.Server("(local)").Database(db_name).TrustedConnection()))
//                    .Mappings(m =>
//                        {
//                            m.HbmMappings.AddFromAssembly(mappings_assembly);
//                            GetMappingsFromExternalAssemblies(m, mappings_assembly, conventions_assembly);
//                        })
//                .ExposeConfiguration(additional_function)
//                .BuildSessionFactory();
//        }

//        /// <summary>
//        /// This loads up FluentNHibernate from the referenced assembly to get the right thing loaded into the conventions - hacky hackity hack hack hack...hack
//        /// This crap will get the right ones, but will not load them.
//        /// </summary>
//        /// <param name="config"></param>
//        /// <param name="mappings_assembly"></param>
//        /// <param name="conventions_assembly"></param>
//        /// <returns></returns>
//         public static FluentMappingsContainer GetMappingsFromExternalAssemblies(MappingConfiguration config, Assembly mappings_assembly, Assembly conventions_assembly)
//        {
//            var mappings = config.FluentMappings;
//            var conventions = mappings.Conventions;
//            var nh_assembly = Assembly.ReflectionOnlyLoadFrom("NHibernate.dll");
//            var fluent_assembly = Assembly.Load("FluentNHibernate");
//            //var fluent_assembly = Assembly.ReflectionOnlyLoadFrom("FluentNHibernate.dll");

//            // mappings 
//            // source.GetTypes()
//            //.Where(x => IsMappingOf<IMappingProvider>(x) ||
//            //            IsMappingOf<IIndeterminateSubclassMappingProvider>(x) ||
//            //            IsMappingOf<IExternalComponentMappingProvider>(x) ||
//            //            IsMappingOf<IFilterDefinition>(x))
//            //.Each(Add);
//            //ISmapping of:  return !type.IsGenericType && typeof(T).IsAssignableFrom(type);

//            var IMappingProvider = fluent_assembly.GetTypes().Where(x => x.IsInterface && x.ToString().Contains("FluentNHibernate.IMappingProvider")).First();
//            var IIndeterminateSubclassMappingProvider = fluent_assembly.GetTypes().Where(x => x.IsInterface && x.ToString().Contains("FluentNHibernate.Mapping.Providers.IIndeterminateSubclassMappingProvider")).First();
//            var IExternalComponentMappingProvider = fluent_assembly.GetTypes().Where(x => x.IsInterface && x.ToString().Contains("FluentNHibernate.Mapping.Providers.IExternalComponentMappingProvider")).First();
//            var IFilterDefinition = fluent_assembly.GetTypes().Where(x => x.IsInterface && x.ToString().Contains("FluentNHibernate.Mapping.IFilterDefinition")).First();

//            foreach (var mappingType in mappings_assembly.GetTypes().Where(x => !x.IsGenericType))
//            {
//                if (IMappingProvider.IsAssignableFrom(mappingType)
//                    || IIndeterminateSubclassMappingProvider.IsAssignableFrom(mappingType)
//                    || IExternalComponentMappingProvider.IsAssignableFrom(mappingType)
//                    || IFilterDefinition.IsAssignableFrom(mappingType)
//                    )
//                {
//                    mappings.Add(mappingType);
//                }
//            }

//            //conventions  if (type.IsAbstract || type.IsGenericType || !typeof(IConvention).IsAssignableFrom(type)) continue; (else add)!)
//            var IConvention = fluent_assembly.GetTypes().Where(x => x.IsInterface && x.ToString().Contains("FluentNHibernate.Conventions.IConvention")).First();
//            foreach (var conventionType in mappings_assembly.GetTypes().Where(x => !x.IsGenericType && !x.IsAbstract))
//            {
//                if (IMappingProvider.IsAssignableFrom(conventionType))
//                {
//                    conventions.Add(conventionType);
//                }
//            }

//            return mappings;
//            //config.FluentMappings.AddFromAssembly(mappings_assembly)
//            //            .Conventions.AddAssembly(conventions_assembly);
//        }

//        private static void no_operation(Configuration cfg) { }

//    }
//}