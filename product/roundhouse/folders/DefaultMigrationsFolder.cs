namespace roundhouse.folders
{
    using infrastructure.filesystem;

    public class DefaultMigrationsFolder : DefaultFolder, MigrationsFolder
    {
        public DefaultMigrationsFolder(FileSystemAccess file_system, string folder_path, string folder_name, bool should_run_items_in_folder_once,bool should_run_items_every_time, string friendly_name)
            : base(file_system, folder_path, folder_name)
        {
            this.should_run_items_in_folder_once = should_run_items_in_folder_once;
            should_run_items_in_folder_every_time = should_run_items_every_time;
            this.friendly_name = friendly_name;
        }

        public bool should_run_items_in_folder_once { get; private set; }
        public bool should_run_items_in_folder_every_time { get; private set; }
        public string friendly_name { get; private set; }
    }
}