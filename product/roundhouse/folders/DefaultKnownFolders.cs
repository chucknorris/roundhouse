namespace roundhouse.folders
{
    public sealed class DefaultKnownFolders : KnownFolders
    {
        public DefaultKnownFolders(
                                   MigrationsFolder alter_database,
                                   MigrationsFolder run_after_create_database,
								   MigrationsFolder run_before_up,
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
            this.run_after_create_database = run_after_create_database;
			this.run_before_up = run_before_up;
            this.up = up;
            this.down = down;
            this.run_first_after_up = run_first_after_up;
            this.functions = functions;
            this.views = views;
            this.sprocs = sprocs;
            this.indexes = indexes;
            this.run_after_other_any_time_scripts = runAfterOtherAnyTimeScripts;
            this.permissions = permissions;
            this.change_drop = change_drop;
        }

        public MigrationsFolder alter_database { get; private set; }
        public MigrationsFolder run_after_create_database { get; private set; }
		public MigrationsFolder run_before_up { get; private set; }
        public MigrationsFolder up { get; private set; }
        public MigrationsFolder down { get; private set; }
        public MigrationsFolder run_first_after_up { get; private set; }
        public MigrationsFolder functions { get; private set; }
        public MigrationsFolder views { get; private set; }
        public MigrationsFolder sprocs { get; private set; }
        public MigrationsFolder indexes { get; private set; }
        public MigrationsFolder run_after_other_any_time_scripts { get; private set; }
        public MigrationsFolder permissions { get; private set; }
        
        public Folder change_drop{get; private set;}
       
    }
}