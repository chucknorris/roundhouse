using roundhouse.console;
using roundhouse.consoles;
using roundhouse.databases.mysql;
using roundhouse.infrastructure.app.logging;
using roundhouse.migrators;
using TestFixtureAttribute = NUnit.Framework.TestFixtureAttribute;
using TestAttribute = NUnit.Framework.TestAttribute;
// ReSharper disable InconsistentNaming

namespace roundhouse.tests.integration
{
	[TestFixture]
	public class MySQLDatabaseTest
	{
		[Test]
		public void Debug_migrator()
		{
			var args = new []
			{
				@"/db=TestRoundhousE",
				@"/f=..\..\db\MySQL\TestRoundhousE",
				@"/cs=server=tcdev02;uid=username;Password=password;database=TestRoundhousE;",
				@"/dt=roundhouse.databases.mysql.MySqlDatabase, roundhouse.databases.mysql",
				@"/schemaname=TestRoundhousE"
			};

			Log4NetAppender.configure();
			Program.run_migrator(Program.set_up_configuration_and_build_the_container(args));
		}

		private const string SqlWithDelimiterKeyword = @"
DROP PROCEDURE IF EXISTS AddSupportAction;
DELIMITER $$

CREATE PROCEDURE AddSupportAction(IN _reservationtID INT, IN _typeCode VARCHAR(45), IN _info VARCHAR(8192), IN _type VARCHAR(100))
BEGIN

 DECLARE actionIdInProc INT;

 INSERT INTO ACTION (Type, TimeStamp, Info)
 VALUES (_type, now(), _info);

 SET actionIdInProc = LAST_INSERT_ID();

 INSERT INTO SUPPORT_ACTION (ActionID, ReservationID, TypeCode)
 VALUES (actionIDInProc, _reservationtID, _typeCode);
 
 SELECT actionIdInProc;

END;
$$";

		[Test]
		public void Debug_StatementSplitter()
		{
			var databaseMigrator = new DefaultDatabaseMigrator(new MySqlDatabase(), null, new ConsoleConfiguration(null));
			var lines = databaseMigrator.get_statements_to_run(SqlWithDelimiterKeyword);
		}
	}
}