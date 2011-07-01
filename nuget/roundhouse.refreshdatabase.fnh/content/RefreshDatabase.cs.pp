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

    public partial class RefreshDatabase
    {
        //https://github.com/chucknorris/roundhouse/wiki/Roundhouserefreshdatabasefnh
        //Note: This should be added to a console application that is set to x86.

        private static bool _isThisInitialDevelopment = true; //are you in greenfield or have you been to production?
        private static string _nameOfScript = "0001_CreateTables_NH.sql"; //name of the script that will be created by nhibernate SchemaExport/SchemaUpdate - this will change the most over time as you move into maintenance

        private static string _databaseName = "__REPLACE__"; //name of your database on your local default instance
        private static string _pathToSqlScripts = @"..\..\..\__REPLACE__"; //This is the path to your scripts folder where Up/Views/Functions/Sprocs are the next folder below. This is a relative path from bin\Debug. The three sets of parent folders already here should get it out of your project folder so you can traverse into the database project folder.
        private static string _repositoryPath = "__REPLACE__"; //The path to your source control repository. Used only for information sake.
        
        //restore 
        private static bool _restoreDuringMaintenance = true; //you want to restore if you have a production backup that is small enough. Otherwise you get into a bit more advanced scenario that this package doesn't cover well
        private static string _pathToRestore = @"\\__REPLACE__.bak"; //this is the path to the restore file, likely on the network so everyone can get to it

        //Note: Add a reference to the project that has the Mappings/Conventions
        private static string _mappingsAssemblyPath = @".\__REPLACE__.dll"; //After adding a reference, the file will be in the build directory, so you can just add the name of the dll here.
        private static string _conventionsAssemblyPath = @".\__REPLACE__.dll"; //If you don't have a conventions assembly, just use the same DLL you just used for mappings.

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

            var migrator = new Migrate().Set(c =>
            {
                c.Logger = new roundhouse.infrastructure.logging.custom.ConsoleLogger();
                c.DatabaseName = _databaseName;
                c.RepositoryPath = _repositoryPath;
                c.SqlFilesDirectory = _pathToSqlScripts;
                c.RestoreFromPath = _pathToRestore;
                c.Silent = true;
                c.RecoveryModeSimple = true;
            });


            var diffType = _restoreDuringMaintenance ? RoundhousEFluentNHDiffingType.MaintenanceWithRestore : RoundhousEFluentNHDiffingType.Maintenance;
            var scriptName = _nameOfScript;
            if (_isThisInitialDevelopment)
            {
                diffType = RoundhousEFluentNHDiffingType.InitialDevelopment;
            }

            new DifferenceWithFluentNHibernate().Run(diffType, scriptName, migrator, mappingsAssembly, conventionsAssembly);
        }
    }
}