using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using roundhouse.console;
using roundhouse.infrastructure.app.logging;
using TestFixtureAttribute = NUnit.Framework.TestFixtureAttribute;
using TestAttribute = NUnit.Framework.TestAttribute;
// ReSharper disable InconsistentNaming

namespace roundhouse.tests.integration
{
	[TestFixture]
	public class MySQLDatabaseTest
	{
		[Test]
		public void T()
		{
			var args = new []
			{
				@"/db=TestRoundhouse",
				@"/f=..\..\db\MySQL\TestRoundhousE",
				@"/cs=server=tcdev02;uid=alexey.diyan;Password=03ambul19G;database=TestRoundhousE;",
				@"/dt=roundhouse.databases.mysql.MySqlDatabase, roundhouse.databases.mysql",
				@"/donotcreatedatabase"
			};

			Log4NetAppender.configure();
			Program.run_migrator(Program.set_up_configuration_and_build_the_container(args));
		}
	}
}