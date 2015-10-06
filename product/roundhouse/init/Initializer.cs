namespace roundhouse.init
{
    using System.IO;
    using folders;
    using infrastructure.app;

    public interface Initializer
    {
        void Initialize(ConfigurationPropertyHolder properties, string path);
    }

    public class FileSystemInitializer : Initializer
    {
        readonly KnownFolders _knownFolders;

        public FileSystemInitializer(KnownFolders knownFolders)
        {
            _knownFolders = knownFolders;
        }

        public void Initialize(ConfigurationPropertyHolder properties, string path)
        {
            makeDir(_knownFolders.alter_database);
            makeDir(_knownFolders.change_drop);
            makeDir(_knownFolders.down);
            makeDir(_knownFolders.functions);
            makeDir(_knownFolders.indexes);
            makeDir(_knownFolders.permissions);
            makeDir(_knownFolders.run_after_create_database);
            makeDir(_knownFolders.run_after_other_any_time_scripts);
            makeDir(_knownFolders.run_before_up);
            makeDir(_knownFolders.run_first_after_up);
            makeDir(_knownFolders.sprocs);
            makeDir(_knownFolders.up);
            makeDir(_knownFolders.views);
        }

        void makeDir(Folder folder)
        {
            if (!Directory.Exists(folder.folder_full_path))
                Directory.CreateDirectory(folder.folder_full_path);
        }
    }
}