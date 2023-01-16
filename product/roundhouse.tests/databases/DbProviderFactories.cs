using System.Data.Common;
using System.Data.SqlClient;
using roundhouse.databases;
using roundhouse.infrastructure.logging.custom;
using Should;

namespace roundhouse.tests.databases
{
    using consoles;
    using roundhouse.databases.sqlserver;
    using roundhouse.databases.sqlserverce;
    using roundhouse.databases.mysql;
#if net462
    using roundhouse.databases.oracle;
    using roundhouse.databases.access;
#endif
    using roundhouse.databases.postgresql;
    using roundhouse.databases.sqlite;
    using roundhouse.infrastructure.app;

    public class DbProviderFactories
    {
        public interface ITestableDatabase
        {
            DbProviderFactory factory { get; }
        }

        public class TestableSqlServerDatabase : SqlServerDatabase, ITestableDatabase
        {
            public DbProviderFactory factory => get_db_provider_factory();
        }

        public class TestableMySqlDatabase : MySqlDatabase, ITestableDatabase
        {
            public DbProviderFactory factory => get_db_provider_factory();
        }

#if net462
        public class TestableOracleDatabase : OracleDatabase, ITestableDatabase
        {
            public DbProviderFactory factory => get_db_provider_factory();
        }

        public class TestableAccessDatabase : AccessDatabase, ITestableDatabase
        {
            public DbProviderFactory factory => get_db_provider_factory();
        }

#endif

        public class TestablePostgreSQLDatabase : PostgreSQLDatabase, ITestableDatabase
        {
            public DbProviderFactory factory => get_db_provider_factory();
        }

        public class TestableRedshiftSQLDatabase : RedshiftSQLDatabase, ITestableDatabase
        {
            public DbProviderFactory factory => get_db_provider_factory();
        }

        public class TestableSqliteDatabase : SqliteDatabase, ITestableDatabase
        {
            public DbProviderFactory factory => get_db_provider_factory();
        }

        public class TestableSqlServer2000Database : roundhouse.databases.sqlserver2000.SqlServerDatabase, ITestableDatabase
        {
            public DbProviderFactory factory => get_db_provider_factory();
        }

        public class TestableSqlServerCEDatabase : SqlServerCEDatabase, ITestableDatabase
        {
            public DbProviderFactory factory => get_db_provider_factory();
        }


        // ReSharper disable once InconsistentNaming
        public abstract class concern_for_Database<TDatabase> : TinySpec<AdoNetDatabase> where TDatabase: AdoNetDatabase, ITestableDatabase, new() 
        {
            protected static ConfigurationPropertyHolder configuration_property_holder;

            public concern_for_Database()
            {
                sut = new TDatabase();
            }

            protected override AdoNetDatabase sut { get; set; }
            protected ITestableDatabase testable_sut() => (TDatabase) sut;

            public override void Context() 
            {
                configuration_property_holder = new DefaultConfiguration
                {
                    Logger = new Log4NetLogFactory().create_logger_bound_to(typeof (DbProviderFactories))
                };
            }

            public override void Because()
            {
                set_database_properties();
                sut.initialize_connections(configuration_property_holder);
            }

            protected virtual void set_database_properties()
            {
                sut.connection_string = "Data Source=|DataDirectory|Northwind.mdb";
                sut.database_name = "Bob";
                sut.server_name = "SQLEXPRESS";
            }
        }

        [Concern(typeof (TestableSqlServerDatabase))]
        public class concern_for_SqlServerDatabase : concern_for_Database<TestableSqlServerDatabase>
        {
            [Observation]
            public void has_sql_server_provider_factory()
            {
                DbProviderFactory fac = testable_sut().factory;
                fac.ShouldBeType<SqlClientFactory>();
            }
        }

        [Concern(typeof(TestableMySqlDatabase))]
        public class concern_for_MySqlDatabase : concern_for_Database<TestableMySqlDatabase>
        {
            [Observation]
            public void has_mysql_provider_factory()
            {
                DbProviderFactory fac = testable_sut().factory;
                fac.ShouldBeType<global::MySql.Data.MySqlClient.MySqlClientFactory>();
            }
        }
        
#if net462
        [Concern(typeof(TestableOracleDatabase))]
        public class concern_for_OracleDatabase : concern_for_Database<TestableOracleDatabase>
        {
            [Observation]
            public void has_oracle_provider_factory()
            {
                DbProviderFactory fac = testable_sut().factory;
                fac.ShouldBeType<System.Data.OracleClient.OracleClientFactory>();
            }
        }
#endif

#if (net462 && _WINDOWS)
        [Concern(typeof(TestableAccessDatabase))]
        public class concern_for_AccessDatabase : concern_for_Database<TestableAccessDatabase>
        {
            protected override void set_database_properties()
            {
                sut.connection_string = "Provider=NorthwindAccess";
            }

            // Fails with Expecting non-empty string for 'providerInvariantName' parameter.
            [Observation]
            public void has_oledb_provider_factory()
            {
                DbProviderFactory fac = testable_sut().factory;
                fac.ShouldBeType<System.Data.OleDb.OleDbFactory>();
            }
        }
#endif

        [Concern(typeof(TestablePostgreSQLDatabase))]
        public class concern_for_PostgresqlDatabase : concern_for_Database<TestablePostgreSQLDatabase>
        {
            [Observation]
            public void has_npgsql_provider_factory()
            {
                DbProviderFactory fac = testable_sut().factory;
                fac.ShouldBeType<Npgsql.NpgsqlFactory>();
            }
        }

#if _WINDOWS
        [Concern(typeof(TestableSqliteDatabase))]
        public class concern_for_SqliteDatabase : concern_for_Database<TestableSqliteDatabase>
        {
            [Observation]
            public void has_sqlite_provider_factory()
            {
                DbProviderFactory fac = testable_sut().factory;
                fac.ShouldBeType<System.Data.SQLite.SQLiteFactory>();
            }
        }
#endif

        [Concern(typeof(TestableSqlServer2000Database))]
        public class concern_for_SqlServer2000Database : concern_for_Database<TestableSqlServer2000Database>
        {
            [Observation]
            public void has_sqlite_provider_factory()
            {
                DbProviderFactory fac = testable_sut().factory;
                fac.ShouldBeType<System.Data.SqlClient.SqlClientFactory>();
            }
        }

#if net462
        [Concern(typeof(TestableSqlServerCEDatabase))]
        public class concern_for_SqlServerCEDatabase : concern_for_Database<TestableSqlServerCEDatabase>
        {
            [Observation]
            public void has_sqlite_provider_factory()
            {
                DbProviderFactory fac = testable_sut().factory;
                fac.ShouldBeType<System.Data.SqlServerCe.SqlCeProviderFactory>();
            }
        }
#endif

    }
}