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
				@"/cs=server=localhost;uid=username;Password=password;database=TestRoundhousE;",
				@"/dt=roundhouse.databases.mysql.MySqlDatabase, roundhouse.databases.mysql",
				@"/schemaname=TestRoundhousE"
			};

			Log4NetAppender.configure();
			Program.run_migrator(Program.set_up_configuration_and_build_the_container(args));
		}
	}
}