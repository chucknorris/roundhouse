//using System.IO;
//using System.Reflection;
//using NUnit.Framework;
//using roundhouse.databases.ravendb.commands;

//namespace roundhouse.tests.databases.ravendb.commands
//{
//    [TestFixture]
//    public class EvalCommandTestFixture
//    {
//        [Test]
//        public void Test()
//        {
//            const string connectionString = "Url=http://lp-inf-199:8080;Database=Condor"; //Raven Server moet actief zijn bij unit tests
//            var scriptToRun = GetFileContent(@"roundhouse.tests.databases.ravendb.commands.EvalCommand.sql");

//            var ravenCommand = RavenCommand.CreateCommand(connectionString, scriptToRun);
            
//            ravenCommand.Execute();
//        }

//        private static string GetFileContent(string resourcePath)
//        {
//            string content = null;
//            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
//            {
//                if (stream != null)
//                {
//                    using (var reader = new StreamReader(stream))
//                    {
//                        content = reader.ReadToEnd();
//                    }
//                }
//            }
//            return content;
//        }
//    }
//}
