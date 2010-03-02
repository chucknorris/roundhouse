namespace roundhouse.nhibernate
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;
    using infrastructure.app;
    using NHibernate.Tool.hbm2ddl;

    public class BuildViaNHibernate
    {
        private readonly ConfigurationPropertyHolder configuration_holder;
        private readonly Dictionary<string, Func<IPersistenceConfigurer>> func_dictionary;

        public BuildViaNHibernate(ConfigurationPropertyHolder config)
        {
            configuration_holder = config;
            func_dictionary = new Dictionary<string, Func<IPersistenceConfigurer>>();
            func_dictionary.Add("sql2005", () => MsSqlConfiguration.MsSql2005.ConnectionString(configuration_holder.ConnectionString));
            func_dictionary.Add("sql2008", () => MsSqlConfiguration.MsSql2008.ConnectionString(configuration_holder.ConnectionString));
        }

        public void Go()
        {
            Fluently.Configure().Database(() => func_dictionary[configuration_holder.DatabaseType]())
                .Mappings(m =>
                          {
                              m.FluentMappings.AddFromAssembly(Assembly.LoadFile("path to assembly"));
                              //conventions
                          })
                .ExposeConfiguration(cfg =>
                                     {
                                         //listeners?
                                         var se = new SchemaExport(cfg);
                                         se.SetOutputFile(Path.Combine(configuration_holder.SqlFilesDirectory,"/up/0001_nhibernate.sql"));
                                         se.Create(true, true);
                                     });
        }
    }
}