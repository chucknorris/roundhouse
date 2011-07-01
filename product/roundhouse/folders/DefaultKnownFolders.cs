namespace roundhouse.folders
{
    public sealed class DefaultKnownFolders : KnownFolders
    {
        public DefaultKnownFolders(
                                   MigrationsFolder alter_database,
                                   MigrationsFolder up,
                                   MigrationsFolder down,
                                   MigrationsFolder run_first_after_up,
                                   MigrationsFolder functions,
                                   MigrationsFolder views,
                                   MigrationsFolder sprocs,
                                   MigrationsFolder indexes,
                                   MigrationsFolder runAfterOtherAnyTimeScripts,        
                                   MigrationsFolder permissions,
                                   Folder change_drop
            )
        {
            this.alter_database = alter_database;
            this.up = up;
            this.down = down;
            this.run_first_after_up = run_first_after_up;
            this.functions = functions;
            this.views = views;
            this.sprocs = sprocs;
            this.indexes = indexes;
            this.runAfterOtherAnyTimeScripts = runAfterOtherAnyTimeScripts;
            this.permissions = permissions;
            this.change_drop = change_drop;
        }

        public MigrationsFolder alter_database { get; private set; }
        public MigrationsFolder up { get; private set; }
        public MigrationsFolder down { get; private set; }
        public MigrationsFolder run_first_after_up { get; private set; }
        public MigrationsFolder functions { get; private set; }
        public MigrationsFolder views { get; private set; }
        public MigrationsFolder sprocs { get; private set; }
        public MigrationsFolder indexes { get; private set; }
        public MigrationsFolder runAfterOtherAnyTimeScripts { get; private set; }
        public MigrationsFolder permissions { get; private set; }
        
        public Folder change_drop{get; private set;}
       
    }
}