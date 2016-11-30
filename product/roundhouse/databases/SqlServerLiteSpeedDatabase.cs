namespace roundhouse.databases
{
    using System;
    using infrastructure.app;
    using infrastructure.persistence;

    public sealed class SqlServerLiteSpeedDatabase : DatabaseDecoratorBase
    {
        public SqlServerLiteSpeedDatabase(Database database) :base(database)
        {
        }

        public override void restore_database(string restore_from_path, string custom_restore_options)
        {
            int current_timeout = command_timeout;
            command_timeout = restore_timeout;
            run_sql(string.Format(
                                 @"USE master 
                        ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                        ALTER DATABASE [{0}] SET MULTI_USER;

                        exec master.dbo.xp_restore_database @database = N'{0}',
                            @filename = N'{1}',
                            @filenumber = 1, 
                            @with = N'RECOVERY', 
                            @with = N'NOUNLOAD',
                            @with = N'REPLACE',
                            @with = N'STATS = 10'
                            {2};

                        ALTER DATABASE [{0}] SET MULTI_USER;
                        ALTER DATABASE [{0}] SET RECOVERY SIMPLE;
                        --DBCC SHRINKDATABASE ([{0}]);
                        ",
                         database_name, restore_from_path,
                         string.IsNullOrEmpty(custom_restore_options) ? string.Empty : ", @with = N'" + custom_restore_options.Replace("'", "''") + "'"
                       ),ConnectionType.Admin);
            command_timeout = current_timeout;
        }

      
    }
}