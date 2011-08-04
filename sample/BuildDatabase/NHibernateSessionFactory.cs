// ==============================================================================
// 
// Fervent Coder Copyright ? 2011 - Released under the Apache 2.0 License
// 
// Copyright 2007-2008 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
//
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
// ==============================================================================
namespace BuildDatabase
{
    using System;
    using System.Reflection;
    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;
    using NHibernate;
    using NHibernate.Cfg;
    using Environment = NHibernate.Cfg.Environment;

    public class NHibernateSessionFactory
    {
        private const string proxy_factory = Environment.ProxyFactoryFactoryClass;
        private const string proxy_factory_name = "NHibernate.ByteCode.Castle.ProxyFactoryFactory";
        private const string db_server = "(local)";

        public static ISessionFactory build_session_factory(string db_name, Assembly mappings_assembly, Assembly conventions_assembly,Action<Configuration> additional_function)
        {
            if (conventions_assembly == null) conventions_assembly = mappings_assembly;
            if (additional_function == null) additional_function = no_operation;

            return
                Fluently.Configure().Database(MsSqlConfiguration.MsSql2008.ConnectionString(c => c.Server(db_server).Database(db_name).TrustedConnection())).
                    Mappings(m => {
                                 m.HbmMappings.AddFromAssembly(mappings_assembly);
                                 m.FluentMappings.AddFromAssembly(mappings_assembly)
                                     .Conventions.AddAssembly(conventions_assembly);
                             }).ExposeConfiguration(cfg => {
                                                        var proxy_factory_location = proxy_factory_name + ", NHibernate.ByteCode.Castle";

                                                        if (cfg.Properties.ContainsKey(proxy_factory))
                                                        {
                                                            cfg.Properties[proxy_factory] = proxy_factory_location;
                                                        }
                                                        else
                                                        {
                                                            cfg.Properties.Add(proxy_factory, proxy_factory_location);
                                                        }
                                                    }).ExposeConfiguration(additional_function).BuildSessionFactory();
        }

        private static void no_operation(Configuration cfg) {}
    }
}