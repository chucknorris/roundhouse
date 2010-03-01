namespace roundhouse.nhibernate
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;
    using infrastructure;
    using NHibernate.Tool.hbm2ddl;

    public class BuildViaNHibernate
    {
        ConfigurationPropertyHolder _config;
        Dictionary<string, Func<IPersistenceConfigurer>> _things;

        public BuildViaNHibernate(ConfigurationPropertyHolder config)
        {
            _config = config;
            _things = new Dictionary<string, Func<IPersistenceConfigurer>>();
            _things.Add("sql2005", () => MsSqlConfiguration.MsSql2005.ConnectionString(_config.ConnectionString));
            _things.Add("sql2008", () => MsSqlConfiguration.MsSql2008.ConnectionString(_config.ConnectionString));
        }

        public void Go()
        {
            Fluently.Configure().Database(() => _things[_config.DatabaseType]())
                .Mappings(m =>
                          {
                              m.FluentMappings.AddFromAssembly(Assembly.LoadFile("path to assembly"));
                              //conventions
                          })
                .ExposeConfiguration(cfg =>
                                     {
                                         //listeners?
                                         var se = new SchemaExport(cfg);
                                         se.SetOutputFile(Path.Combine(_config.SqlFilesDirectory,"/up/0001_nhibernate.sql"));
                                         se.Create(true, true);
                                     });
        }
    }
}