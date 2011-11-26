namespace roundhouse.infrastructure.app
{
    using extensions;

    public class DatabaseTypeSynonyms
    {
        public static string convert_database_type_synonyms(string database_type)
        {
            string database_type_full_name = database_type;

            switch (database_type.to_lower())
            {
                case "2008":
                case "sql2008":
                case "sqlserver2008":
                case "2005":
                case "sql2005":
                case "sqlserver2005":
                case "sql":
                case "sql.net":
                case "sqlserver":
                case "sqlado.net":
                    database_type_full_name =
                        "roundhouse.databases.sqlserver.SqlServerDatabase, roundhouse.databases.sqlserver";
                    break;
                case "2000":
                case "sql2000":
                case "sqlserver2000":
                    database_type_full_name =
                        "roundhouse.databases.sqlserver2000.SqlServerDatabase, roundhouse.databases.sqlserver2000";
                    break;
                case "mysql":
                    database_type_full_name =
                        "roundhouse.databases.mysql.MySqlDatabase, roundhouse.databases.mysql";
                    break; 
                case "sqlite":
                    database_type_full_name =
                        "roundhouse.databases.sqlite.SqliteDatabase, roundhouse.databases.sqlite";
                    break;
                case "oracle":
                    database_type_full_name =
                        "roundhouse.databases.oracle.OracleDatabase, roundhouse.databases.oracle";
                    break;
                case "access":
                    database_type_full_name = "roundhouse.databases.access.AccessDatabase, roundhouse.databases.access";
                    break;
                case "pg":
                case "postgres":
                case "postgresql":
                    database_type_full_name = "roundhouse.databases.postgresql.PostgreSQLDatabase, roundhouse.databases.postgresql";
                    break;
                    //case "oledb":
                    //    database_type_full_name =
                    //        "roundhouse.databases.oledb.OleDbDatabase, roundhouse.databases.oledb";
                    //    break;
            }

            return database_type_full_name;
        }
    }
}