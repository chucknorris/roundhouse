using roundhouse.databases;
using roundhouse.infrastructure.app;

namespace roundhouse.migrators
{
    using environments;

    public interface DatabaseMigrator
    {
        Database database { get; set; }
        void initialize_connections();
        void open_admin_connection();
        void close_admin_connection();
        void open_connection(bool with_transaction);
        void close_connection();
        void backup_database_if_it_exists();
        bool create_or_restore_database(string custom_create_database_script);
        void set_recovery_mode(bool simple);
        //void restore_database(string restore_from_path);
        void delete_database();
        void run_roundhouse_support_tasks();
        string get_current_version(string repository_path);
        long version_the_database(string repository_path, string repository_version);
        bool run_sql(string sql_to_run, string script_name, bool run_this_script_once, bool run_this_script_every_time, long version_id, Environment migrating_environment, string repository_version, string repository_path, ConnectionType connection_type);
        //void transfer_to_database_for_changes();
    }
}