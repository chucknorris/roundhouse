using System.Collections.Generic;

namespace roundhouse.sql
{
    public static class SqlScripts
    {
        public static SqlScript t_sql_scripts = new TSQLScript();
        public static SqlScript t_sql2000_scripts = new TSQL2000Script();
        public static SqlScript access_sql_scripts = new AccessSQLScript();
        public static SqlScript pl_sql_scripts = new PLSQLScript();
        
        public static IDictionary<string, SqlScript> sql_scripts_dictionary = generate_scripts_dictionary();

        private static IDictionary<string, SqlScript> generate_scripts_dictionary()
        {
            IDictionary<string, SqlScript> scripts_dictionary = new Dictionary<string, SqlScript>();

            scripts_dictionary.Add("SQLServer", t_sql_scripts);
            scripts_dictionary.Add("System.Data.SqlClient", t_sql_scripts);
            scripts_dictionary.Add("SQLNCLI", t_sql_scripts);
            scripts_dictionary.Add("SQLNCLI10", t_sql_scripts);
            scripts_dictionary.Add("sqloledb", t_sql_scripts);
            scripts_dictionary.Add("Microsoft.SQLSERVER.MOBILE.OLEDB.3.0", t_sql_scripts);
            scripts_dictionary.Add("Microsoft.SQLSERVER.CE.OLEDB.3.5", t_sql_scripts);
            scripts_dictionary.Add("Microsoft.Jet.OLEDB.4.0", access_sql_scripts);
            scripts_dictionary.Add("Microsoft.ACE.OLEDB.12.0", access_sql_scripts);
            scripts_dictionary.Add("Oracle", pl_sql_scripts);
            scripts_dictionary.Add("SQLServer2000", t_sql2000_scripts);
            //scripts_dictionary.Add("MySQLProv", mysql_sql_scripts);
            //scripts_dictionary.Add("MyOracleDB", oracle_sql_scripts);
            //scripts_dictionary.Add("msdaora", oracle_sql_scripts);
            //scripts_dictionary.Add("OraOLEDB.Oracle", oracle_sql_scripts);
            //scripts_dictionary.Add("vfpoledb", vfp_sql_scripts);
            //scripts_dictionary.Add("IBMDADB2", db2_sql_scripts);
            //scripts_dictionary.Add("DB2OLEDB", db2_sql_scripts);
            //scripts_dictionary.Add("PostgreSQL OLE DB Provider", postgre_sql_scripts);

            return scripts_dictionary;
        }


    }
}