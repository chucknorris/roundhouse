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
    using System.Reflection;
    using roundhouse;

    public class RefreshDatabase
    {
        //Note: This should be added to a console application that is set to x86.

        private static bool _isThisInitialDevelopment = true;
        private static string _nameOfInitialScript = "0001_CreateTables_NH.sql";
        private static string _nameOfUpdateScript = "0002_AlterTables_NH.sql";

        private static string _databaseName = "__REPLACE__";
        private static string _pathToSqlScripts = @"..\..\..\__REPLACE__";
        private static string _repositoryPath = "__REPLACE__";

        private static bool _restoreDuringMaintenance = true;
        private static string _pathToRestore = @"\\__REPLACE__.bak";

        //Note: Add a reference to the project that has the Mappings/Conventions
        private static string _mappingsAssemblyPath = @".\__REPLACE__.dll";
        private static string _conventionsAssemblyPath = @".\__REPLACE__.dll";

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

            var mappingsAssembly = Assembly.LoadFrom(_mappingsAssemblyPath);
            var conventionsAssembly = Assembly.LoadFrom(_conventionsAssemblyPath);

            var migrator = new Migrate().Set(c => {
                                                c.DatabaseName = _databaseName;
                                                c.RepositoryPath = _repositoryPath;
                                                c.SqlFilesDirectory = _pathToSqlScripts;
                                                c.RestoreFromPath = _pathToRestore;
                                                c.Silent = true;
                                                c.RecoveryModeSimple = true;
                                             });


            var diffType = _restoreDuringMaintenance ? RoundhousEFluentNHDiffingType.MaintenanceWithRestore : RoundhousEFluentNHDiffingType.Maintenance;
            var scriptName = _nameOfUpdateScript;
            if (_isThisInitialDevelopment)
            {
                scriptName = _nameOfInitialScript;
                diffType = RoundhousEFluentNHDiffingType.InitialDevelopment;
            }

            new DifferenceWithFluentNHibernate().Run(diffType, scriptName, migrator, mappingsAssembly, conventionsAssembly);
        }
    }
}