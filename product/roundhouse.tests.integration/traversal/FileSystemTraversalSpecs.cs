﻿// ReSharper disable InconsistentNaming

using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using bdddoc.core;
using developwithpassion.bdd.contexts;
using developwithpassion.bdd.mbunit.standard;
using developwithpassion.bdd.mbunit.standard.observations;
using MbUnit.Framework;
using Rhino.Mocks;
using roundhouse.folders;
using roundhouse.infrastructure.app;
using roundhouse.infrastructure.app.builders;
using roundhouse.infrastructure.filesystem;
using roundhouse.traversal;

namespace roundhouse.tests.integration.traversal
{
    
    public class FileSystemTraversalSpecs
    {
        public abstract class concern_for_file_traversal : observations_for_a_static_sut
        {

            private context c = () => { };
            private before_all_observations create_files = () =>
            {
                file_system = new TemporaryDirectory().with(temp =>
                    {
                        temp.has_subdirectory_named("up").with(up => 
                            up.has_files_named("0001_CreateTables.sql", "0002_ChangeTable.sql", "0003_TestBatchSplitter.sql").with_random_content());
                        temp.has_subdirectory_named("functions").with(func => func.has_file_named("ufn_GetDate.sql").with_random_content());
                        temp.has_subdirectory_named("views").with(views => views.has_file_named("vw_Dude.sql"));
                        temp.has_subdirectory_named("sprocs").with(sprocs => sprocs.has_files_named("usp_GetDate.sql", "usp_SelectTimmy.sql").with_random_content());
                        temp.has_subdirectory_named("runAfterOtherAnyTimeScripts").with(ra => ra.has_file_named("createFiveItems.sql").with_random_content());
                    	temp.has_subdirectory_named("permissions").with(p =>
                    	    {
                    	        p.has_subdirectory_named("sub").with(
									s => s.has_file_named("0002_AppReadOnlyRole.sql").with_random_content()
								);

                    	    	p.has_files_named("0001_AppRole.sql",
                    	    	                  "0003_AppPermissionsWiring.sql")
                    	    		.with_random_content();
                    	    });
                    }
                );
            };
            private after_all_observations kill_files = () => file_system.Dispose();

            protected static TemporaryDirectory file_system;

            protected static object result;



        }

        [Concern(typeof(FileSystemTraversal))]
        public class when_traversing_file_system : concern_for_file_traversal
        {
            because b = () => { };

            [Observation]
            public void if_told_to_traverse_all_items_expect_that_all_folders_were_traversed()
            {
                FileSystemAccess access = new WindowsFileSystemAccess();
                ConfigurationPropertyHolder configuration = new consoles.DefaultConfiguration
                                                                {
                                                                    SqlFilesDirectory = file_system.directory.FullName,
                                                                    DatabaseName = "test"
                                                                };
                ApplicationConfiguraton.set_defaults_if_properties_are_not_set(configuration);
                KnownFolders folders = KnownFoldersBuilder.build(access, configuration);
                FileSystemTraversal traversal = new FileSystemTraversal(folders, access, false);
                
                List<string> traversedfolders = new List<string>();

                traversal.traverse(t =>
                                       {
                                           t.include_all_folders();
                                           t.before_each_folder(f => traversedfolders.Add(f.folder_name));
                                       });
                Assert.In("up", traversedfolders);
                Assert.In("functions", traversedfolders);
                Assert.In("views", traversedfolders);
                Assert.In("sprocs", traversedfolders);
                Assert.In("runAfterOtherAnyTimeScripts", traversedfolders);
                Assert.In("permissions", traversedfolders);
            }

			[Observation]
            public void if_told_to_traverse_width_first_expect_width_first_order()
            {
                FileSystemAccess access = new WindowsFileSystemAccess();
                ConfigurationPropertyHolder configuration = new consoles.DefaultConfiguration
                                                                {
                                                                    SqlFilesDirectory = file_system.directory.FullName,
                                                                    DatabaseName = "test"
                                                                };
                ApplicationConfiguraton.set_defaults_if_properties_are_not_set(configuration);
                KnownFolders folders = KnownFoldersBuilder.build(access, configuration);
                FileSystemTraversal traversal = new FileSystemTraversal(folders, access, false);
                
                List<string> scripts = new List<string>();

                traversal.traverse(t =>
                                       {
                                           t.include_folder(folders.permissions);
                                           t.for_each_script(s => scripts.Add(s.script_name));
                                       });

				Assert.AreEqual(3, scripts.Count);
				Assert.AreEqual("0001_AppRole.sql", scripts[0]);
				Assert.AreEqual("0003_AppPermissionsWiring.sql", scripts[1]);
				Assert.AreEqual("0002_AppReadOnlyRole.sql", scripts[2]);
            }

			[Observation]
            public void if_told_to_traverse_depth_first_expect_depth_first_order()
            {
                FileSystemAccess access = new WindowsFileSystemAccess();
                ConfigurationPropertyHolder configuration = new consoles.DefaultConfiguration
                                                                {
                                                                    SqlFilesDirectory = file_system.directory.FullName,
                                                                    DatabaseName = "test"
                                                                };
                ApplicationConfiguraton.set_defaults_if_properties_are_not_set(configuration);
                KnownFolders folders = KnownFoldersBuilder.build(access, configuration);
                FileSystemTraversal traversal = new FileSystemTraversal(folders, access, true);
                
                List<string> scripts = new List<string>();

                traversal.traverse(t =>
                                       {
                                           t.include_folder(folders.permissions);
                                           t.for_each_script(s => scripts.Add(s.script_name));
                                       });

				Assert.AreEqual(3, scripts.Count);
				Assert.AreEqual("0001_AppRole.sql", scripts[0]);
				Assert.AreEqual("0002_AppReadOnlyRole.sql", scripts[1]);
				Assert.AreEqual("0003_AppPermissionsWiring.sql", scripts[2]);
            }
        }
    }
}
