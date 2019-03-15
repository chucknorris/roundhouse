using System;
using System.Collections.Generic;
using NUnit.Framework;
using roundhouse.databases.mysql.parser;
using roundhouse.sqlsplitters;

namespace roundhouse.tests.sqlsplitters
{
    public class MySqlStatementSplitterSpecs
    {
        public abstract class concern_for_StatementSplitter : TinySpec
        {
            public override void Context()
            {
                
            }
        }

        [Concern(typeof(StatementSplitter))]
        public class when_splitting_sql_statements : concern_for_StatementSplitter
        {
            public override void Because()
            {

            }

            [Observation]
            public void should_split_on_delimiters()
            {
                string script = "select * from test1;\nselect * from test2;\n";

                Parser parser = new Parser(script);
                List<ParsedStatement> statements = parser.Parse();
                WriteStatements(statements);

                WriteOutput("Statements parsed: " + statements.Count);
                Assert.AreEqual("select * from test1" + Environment.NewLine, statements[0].Value);
                Assert.AreEqual(2, statements.Count);
            }

            [Observation]
            public void should_retain_comments()
            {
                string script = "/*\n *\n * Test Statement\n *\n */\nselect * from test1;\nselect * from test2;\n";

                Parser parser = new Parser(script);
                List<ParsedStatement> statements = parser.Parse();
                WriteStatements(statements);

                Assert.AreEqual("/*\n *\n * Test Statement\n *\n */\nselect * from test1", statements[0].Value.Trim());
                Assert.AreEqual(2, statements.Count);
            }

            [Observation]
            public void should_honor_single_character_delimiter()
            {
                string script = "delimiter $\nselect * from test1$\nselect * from test2$\n";

                Parser parser = new Parser(script);
                List<ParsedStatement> statements = parser.Parse();
                WriteStatements(statements);

                WriteOutput("Statements parsed: " + statements.Count);
                Assert.AreEqual("select * from test1" + Environment.NewLine, statements[0].Value);
                Assert.AreEqual(2, statements.Count);
            }

            [Observation]
            public void should_honor_tricky_single_character_delimiter()
            {
                string script = "delimiter Z\nselect * from test1Z\nselect * from test2Z\n";

                Parser parser = new Parser(script);
                List<ParsedStatement> statements = parser.Parse();
                WriteStatements(statements);

                WriteOutput("Statements parsed: " + statements.Count);
                Assert.AreEqual("select * from test1" + Environment.NewLine, statements[0].Value);
                Assert.AreEqual(2, statements.Count);
            }

            [Observation]
            public void should_honor_double_character_delimiter()
            {
                string script = "delimiter $$\nselect * from test1$$\nselect * from test2$$\n";

                Parser parser = new Parser(script);
                List<ParsedStatement> statements = parser.Parse();
                WriteStatements(statements);

                WriteOutput("Statements parsed: " + statements.Count);
                Assert.AreEqual("select * from test1" + Environment.NewLine, statements[0].Value);
                Assert.AreEqual(2, statements.Count);
            }

            [Observation]
            public void should_honor_tricky_double_character_delimiter()
            {
                string script = "delimiter ZZ\nselect * from test1ZZ\nselect * from test2ZZ\n";

                Parser parser = new Parser(script);
                List<ParsedStatement> statements = parser.Parse();
                WriteStatements(statements);

                WriteOutput("Statements parsed: " + statements.Count);
                Assert.AreEqual("select * from test1" + Environment.NewLine, statements[0].Value);
                Assert.AreEqual(2, statements.Count);
            }

            [Observation]
            public void should_honor_multicharacter_delimiter()
            {
                string script = "delimiter E4TMY$H0RT$\nselect * from test1E4TMY$H0RT$\nselect * from test2E4TMY$H0RT$\n";

                Parser parser = new Parser(script);
                List<ParsedStatement> statements = parser.Parse();
                WriteStatements(statements);

                WriteOutput("Statements parsed: " + statements.Count);
                Assert.AreEqual("select * from test1" + Environment.NewLine, statements[0].Value);
                Assert.AreEqual(2, statements.Count);
            }

            [Observation]
            public void should_honor_trailing_delimiter()
            {
                string script = "delimiter $$\nselect * from test1$$\nselect * from test2$$select * from test3$$\ndelimiter ;";

                Parser parser = new Parser(script);
                List<ParsedStatement> statements = parser.Parse();
                WriteStatements(statements);

                WriteOutput("Statements parsed: " + statements.Count);
                Assert.AreEqual("select * from test1" + Environment.NewLine, statements[0].Value);
                Assert.AreEqual(3, statements.Count);
            }

            [Observation]
            public void should_honor_quoted_semicolon()
            {
                string script = "set ps = 'select * from test;';\nprepare ps;\nexecute ps;";

                Parser parser = new Parser(script);
                List<ParsedStatement> statements = parser.Parse();
                WriteStatements(statements);

                WriteOutput("Statements parsed: " + statements.Count);
                Assert.AreEqual("set ps = 'select * from test;'" + Environment.NewLine, statements[0].Value);
                Assert.AreEqual(3, statements.Count);
            }

            private void WriteStatements(List<ParsedStatement> statements) {
                int index = 0;
                foreach (ParsedStatement statement in statements) {
                    WriteOutput(index + ":");
                    WriteStatement(statement);
                    index++;
                }
            }

            private void WriteStatement(ParsedStatement statement) {
                WriteOutput(statement.Value);
            }

            private void WriteOutput(string value) {
                //NUnit.Framework.TestContext.Progress.WriteLine(value);
            }
        }
    }
}