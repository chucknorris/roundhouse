// ==============================================================================
// 
// Fervent Coder Copyright © 2011 - Released under the Apache 2.0 License
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
namespace $rootnamespace$
{
    using System;
    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;
    using log4net;
    using NHibernate;
    using NHibernate.Cfg;

    public class NHibernateSessionFactory
    {
        #region Constants

        private static readonly ILog logger = LogManager.GetLogger(typeof (NHibernateSessionFactory));

        #endregion

        #region Methods

        public static ISessionFactory build_session_factory<TYPE, CONVENTIONS_TYPE>(string db_server, string db_name, Action<Configuration> additional_function)
        {
            logger.Debug("Building Session Factory");
            return Fluently.Configure().Database(MsSqlConfiguration.MsSql2008.ConnectionString(c => c.Server(db_server).Database(db_name).TrustedConnection())).Mappings(m =>
            {
                //m.HbmMappings.AddFromAssemblyOf<TYPE>()
                m.FluentMappings.AddFromAssemblyOf<TYPE>().Conventions.AddFromAssemblyOf<CONVENTIONS_TYPE>();
            }).ExposeConfiguration(additional_function).BuildSessionFactory();
        }

        //#region new stuff

        //public static ISessionFactory build_session_factory(string db_server, string db_name, Assembly fluent_mapping_assembly, Assembly fluent_convention_assembly, Action<Configuration> additional_function)
        //{
        //    logger.Debug("Building Session Factory");
        //    return Fluently.Configure()
        //        .Database(MsSqlConfiguration.MsSql2005
        //                      .ConnectionString(c => c.Server(db_server).Database(db_name).TrustedConnection())
        //        )
        //        .Mappings(m =>
        //                      {
        //                          register_maps_and_conventions(m, fluent_mapping_assembly, fluent_convention_assembly);
        //                      })
        //        .ExposeConfiguration(additional_function)
        //        .BuildSessionFactory();

        //}

        //private static FluentMappingsContainer register_maps_and_conventions(MappingConfiguration mapping_configuration, Assembly fluent_mapping_assembly, Assembly fluent_convention_assembly)
        //{
        //    FluentMappingsContainer fluent_mappings = mapping_configuration.FluentMappings;
        //    fluent_mappings.AddFromAssembly(fluent_mapping_assembly);
        //    if (fluent_convention_assembly != null)
        //    {
        //        fluent_mappings.Conventions.AddAssembly(fluent_convention_assembly);
        //    }

        //    return fluent_mappings;
        //}
        //#endregion

        private static void no_operation(Configuration cfg) {}

        #endregion
    }
}