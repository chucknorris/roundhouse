using MySql.Data.MySqlClient;

namespace roundhouse.tests.integration.databases
{
    public interface DatabaseAsserts
    {
        void assert_table_exists(string table_name);
        void assert_table_not_exists(string table_name);
        int one_time_scripts_run();
    }
}