using roundhouse.folders;

namespace roundhouse.tests
{
    class MockKnownFolders: KnownFolders
    {
        public MigrationsFolder alter_database
        {
            get { return get_migrationsfolder("alter_database"); }
        }

        public MigrationsFolder run_after_create_database 
        {
            get { return get_migrationsfolder("run_after_create_database"); }
        }
        public MigrationsFolder run_before_up 
        {
            get { return get_migrationsfolder("run_before_up"); }
        }
        public MigrationsFolder up 
        {
            get { return get_migrationsfolder("up"); }
        }
        public MigrationsFolder down 
        {
            get { return get_migrationsfolder("down"); }
        }
        public MigrationsFolder run_first_after_up 
        {
            get { return get_migrationsfolder("run_first_afte_up"); }
        }
        public MigrationsFolder functions 
        {
            get { return get_migrationsfolder("functions"); }
        }
        public MigrationsFolder views 
        {
            get { return get_migrationsfolder("views"); }
        }
        public MigrationsFolder sprocs 
        {
            get { return get_migrationsfolder("sprocs"); }
        }
        public MigrationsFolder triggers 
        {
            get { return get_migrationsfolder("triggers"); }
        }
        public MigrationsFolder indexes 
        {
            get { return get_migrationsfolder("indexes"); }
        }
        public MigrationsFolder run_after_other_any_time_scripts 
        {
            get { return get_migrationsfolder("run_after_other_any_time_scrips"); }
        }
        public MigrationsFolder permissions 
        {
            get { return get_migrationsfolder("permissions"); }
        }
        public MigrationsFolder before_migration 
        {
            get { return get_migrationsfolder("before_migration"); }
        }
        public MigrationsFolder after_migration 
        {
            get { return get_migrationsfolder("after_migration"); }
        }
        public Folder change_drop 
        {
            get { return get_migrationsfolder("change_drop"); }
        }

        private MigrationsFolder get_migrationsfolder(string name)
        {
            return new MockMigrationsFolder(name);
        }

        public class MockMigrationsFolder: MigrationsFolder
        {
            public MockMigrationsFolder(string folder_name)
            {
                this.folder_name = folder_name;
                this.folder_path = "folder_prefix\\" + folder_name;
                this.folder_full_path = "drive_and_source_structure\\" + folder_path;
                this.friendly_name = "friendly " + folder_name;
            }

            public string folder_name { get; set; }

            public string folder_path { get; private set; }

            public string folder_full_path { get; private set; }

            public bool should_run_items_in_folder_once { get; private set; }

            public bool should_run_items_in_folder_every_time { get; private set; }

            public string friendly_name { get; private set; }
        }

    }
}
