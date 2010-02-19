using roundhouse.databases;

namespace roundhouse.migrators
{
    using environments;

    public interface DatabaseMigrator
    {
        Database database { get; set; }
        void connect(bool with_transaction);
        void disconnect();
        void backup_database_if_it_exists();
        void create_or_restore_database();
        void set_recovery_mode(bool simple);
        //void restore_database(string restore_from_path);
        void delete_database();
        void verify_or_create_roundhouse_tables();
        string get_current_version(string repository_path);
        long version_the_database(string repository_path, string repository_version);
        bool run_sql(string sql_to_run, string script_name, bool run_this_script_once, bool run_this_script_every_time, long version_id, Environment migrating_environment);
        void transfer_to_database_for_changes();
    }
}