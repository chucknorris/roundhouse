namespace roundhouse.folders
{
    public interface KnownFolders
    {
        MigrationsFolder up { get; }
        MigrationsFolder down { get; }
        MigrationsFolder run_first_after_up { get; }
        MigrationsFolder functions { get; }
        MigrationsFolder views { get; }
        MigrationsFolder sprocs { get; }
        MigrationsFolder runAfterOtherAnyTimeScripts { get; }
        MigrationsFolder permissions { get; }
        Folder change_drop { get; }
    }
}