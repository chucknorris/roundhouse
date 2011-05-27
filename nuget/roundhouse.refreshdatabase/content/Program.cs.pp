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
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using NHibernate.Tool.hbm2ddl;
    using Configuration = NHibernate.Cfg.Configuration;
    using ONEOFYOURMAPPINGCLASSES = __REPLACE__;
    using ONEOFYOURCONVENTIONCLASSES = __REPLACE__;

    internal class Program
    {
        #region Constants

        private static string _dbServer = "(local)";
        private static string _dbName = "__REPLACE__";
        private static string _pathToScripts = "..\..\..\__REPLACE__";
        private static string _nameOfScript = "0001_CreateTables_NH.sql";
        private static string _nameOfUpdateScript = "0002_AlterTables_NH.sql";
        private static bool _initialDevelopment = true;
        private static string _pathToRestore = "__REPLACE__";
        private static string _repositoryPath = "__REPLACE__";

        #endregion

        #region Methods

        private static void Main(string[] args)
        {
            try
            {
                RunRoundhouseNhibernate();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }

        private static void RunRoundhouseNhibernate()
        {
            if (_initialDevelopment)
            {
                RunInitialDatabaseSetup();
            }
            else
            {
                RunMaintenanceDatabaseSetup();
            }
        }

        // initial database setup

        public static void RunInitialDatabaseSetup()
        {
            CreateTheDatabase(_roundhouseExe, _dbServer, _dbName);
            BuildDatabaseSchema(_dbServer, _dbName);
            RunRoundhouseDropCreate(_roundhouseExe, _dbServer, _dbName, _pathToScripts, _repositoryPath);
        }

        private static void CreateTheDatabase(string roundhouseExe, string serverName, string dbName)
        {
            CommandRunner.run(roundhouseExe, string.Format("/s={0} /db={1} /f={2} /silent /simple", serverName, dbName, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)), true);
        }

        private static void BuildDatabaseSchema(string dbServer, string dbName)
        {
            //Assembly mapping_assembly = Assembly.LoadFile(Path.GetFullPath(MAPPINGS_ASSEMBLY));
            //Assembly convention_assembly = Assembly.LoadFile(Path.GetFullPath(CONVENTIONS_ASSEMBLY));

            //ISessionFactory sf = NHibernateSessionFactory.build_session_factory(db_server, db_name,mapping_assembly,convention_assembly, build_schema);
            var sf = NHibernateSessionFactory.build_session_factory<ONEOFYOURMAPPINGCLASSES, ONEOFYOURCONVENTIONCLASSES>(dbServer, dbName, BuildSchema);
        }

        private static void BuildSchema(Configuration cfg)
        {
            var s = new SchemaExport(cfg);
            s.SetOutputFile(Path.Combine(_pathToScripts, Path.Combine("Up", _nameOfScript)));
            s.Create(true, false);
        }

        private static void RunRoundhouseDropCreate(string roundhouseExe, string serverName, string dbName, string pathToScripts, string repositoryPath)
        {
            CommandRunner.run(roundhouseExe, string.Format("/s={0} /db={1} /f=. /silent /drop", serverName, dbName), true);
            CommandRunner.run(roundhouseExe, string.Format("/s={0} /db={1} /f={2} /r={3} /silent /simple", serverName, dbName, pathToScripts, repositoryPath), true);
        }

        // maintenance database setup

        public static void RunMaintenanceDatabaseSetup()
        {
            RestoreTheDatabase(_roundhouseExe, _dbServer, _dbName, _pathToRestore);
            UpgradeDatabaseSchema(_dbServer, _dbName);
            RunRoundhouseUpdates(_roundhouseExe, _dbServer, _dbName, _pathToScripts, _repositoryPath);
        }

        private static void RestoreTheDatabase(string roundhouseExe, string serverName, string dbName, string pathToRestore)
        {
            CommandRunner.run(roundhouseExe, string.Format("/s={0} /db={1} /f=. /silent /restore /restorefrompath=\"{2}\"", serverName, dbName, pathToRestore), true);
        }

        private static void UpgradeDatabaseSchema(string dbServer, string dbName)
        {
            var sf = NHibernateSessionFactory.build_session_factory<ONEOFYOURMAPPINGCLASSES, ONEOFYOURCONVENTIONCLASSES>(dbServer, dbName, UpdateSchema);
        }

        public static void UpdateSchema(Configuration cfg)
        {
            var s = new SchemaUpdate(cfg);
            var sb = new StringBuilder();
            s.Execute(x => sb.Append(x), false);
            var updateScriptFileName = Path.Combine(_pathToScripts, Path.Combine("up", _nameOfUpdateScript));
            if (File.Exists(updateScriptFileName))
            {
                File.Delete(updateScriptFileName);
            }
            File.WriteAllText(updateScriptFileName, sb.ToString());
        }

        private static void RunRoundhouseUpdates(string roundhouseExe, string serverName, string dbName, string pathToScripts, string repositoryPath)
        {
            CommandRunner.run(roundhouseExe, string.Format("/s={0} /db={1} /f={2} /r={3} /silent", serverName, dbName, pathToScripts, repositoryPath), true);
        }

        #endregion
    }
}