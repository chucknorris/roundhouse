using System.Linq;
using log4net.Appender;

namespace roundhouse.tests.sqlsplitters
{
    using System;
    using System.Text.RegularExpressions;
    using bdddoc.core;
    using developwithpassion.bdd.contexts;
    using developwithpassion.bdd.mbunit.standard;
    using developwithpassion.bdd.mbunit.standard.observations;
    using MbUnit.Framework;
    using roundhouse.databases;
    using roundhouse.databases.access;
    using roundhouse.databases.oracle;
    using roundhouse.databases.sqlserver;
    using roundhouse.sqlsplitters;

    public class StatementSplitterSpecs
    {
        public abstract class concern_for_StatementSplitter : observations_for_a_static_sut
        {
            protected static StatementSplitter splitter;
            protected static string access_sql_separator_regex_string;
            protected static string plsql_separator_regex_string;

            private context c = () =>
                                    {
                                        Database database = new AccessDatabase();
                                        access_sql_separator_regex_string = database.sql_statement_separator_regex_pattern;
                                        database = new OracleDatabase();
                                        plsql_separator_regex_string = database.sql_statement_separator_regex_pattern;
                                    };
        }

        [Concern(typeof(StatementSplitter))]
        public class when_replacing_tsql_statements_with_the_statement_splitters_match_evaluator : concern_for_StatementSplitter
        {
            protected static string symbols_to_check = "`~!@#$%^&*()-_+=,.;:\"[]\\/?<>";
            protected static string words_to_check = "abcdefghijklmnopqrstuvwzyz0123456789 ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            private because b = () =>
                                    {
                                        Database database = new SqlServerDatabase();
                                        splitter = database.sql_splitter;
                                    };

            [Observation]
            public void should_replace_on_lowercase_go_statement()
            {
                string sql_to_match = "\r\nwhere DataName = 'AttributeKeyMap'\r\ngo ";
                Console.WriteLine(sql_to_match);
                var expected = new[] { "\r\nwhere DataName = 'AttributeKeyMap'\r\n" };

                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_full_statement_without_issue()
            {
                string sql_to_match = SplitterContext.FullSplitter.tsql_statement;
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(SplitterContext.FullSplitter.tsql_statement_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_space()
            {
                const string sql_to_match = @" GO ";
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                Assert.IsEmpty(sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_tab()
            {
                string sql_to_match = @" GO" + string.Format("\t");
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                Assert.IsEmpty(sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_new_line()
            {
                const string sql_to_match = @" GO
";
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                Assert.IsEmpty(sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_on_new_line_after_double_dash_comments()
            {
                const string sql_to_match =
                    @"--
GO
";
                var expected_scrubbed = new [] {
                    @"--
"};
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_on_new_line_after_double_dash_comments_and_words()
            {
                string sql_to_match = @"-- " + words_to_check + @"
GO
";
                var expected_scrubbed = new [] { @"-- " + words_to_check + @"
"};
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray(); 
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_on_new_line_after_double_dash_comments_and_symbols()
            {
                string sql_to_match = @"-- " + symbols_to_check + @"
GO
";
                var expected_scrubbed =  new [] { @"-- " + symbols_to_check + @"
"};
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_on_its_own_line()
            {
                const string sql_to_match = @" 
GO
";
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                Assert.IsEmpty(sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_no_line_terminator()
            {
                const string sql_to_match = @" GO ";
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                Assert.IsEmpty(sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_words_before()
            {
                string sql_to_match = words_to_check + @" GO
";
                var expected_scrubbed = new [] { words_to_check + @" "};
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_symbols_and_words_before()
            {
                string sql_to_match = symbols_to_check + words_to_check + @" GO
";
                var expected_scrubbed = new [] { symbols_to_check + words_to_check + @" "};
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_words_and_symbols_before()
            {
                string sql_to_match = words_to_check + symbols_to_check + @" GO
";
                var expected_scrubbed = new [] { words_to_check + symbols_to_check + @" "};
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_words_after_on_the_same_line()
            {
                string sql_to_match = @" GO " + words_to_check;
                var expected_scrubbed = new [] { @" " + words_to_check };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_words_after_on_the_same_line_including_symbols()
            {
                string sql_to_match = @" GO " + words_to_check + symbols_to_check;
                var expected_scrubbed = new [] {@" " + words_to_check + symbols_to_check };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_words_before_and_after_on_the_same_line()
            {
                string sql_to_match = words_to_check + @" GO " + words_to_check;
                var expected_scrubbed = new [] { words_to_check + @" " , @" " + words_to_check };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_words_before_and_after_on_the_same_line_including_symbols()
            {
                string sql_to_match = words_to_check + symbols_to_check.Replace("'","").Replace("\"","") + " GO BOB" + symbols_to_check;
                var expected_scrubbed = new [] { words_to_check + symbols_to_check.Replace("'", "").Replace("\"", "") + " " , " BOB" + symbols_to_check};
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_preceeding_line_having_comment_and_apostrophe()
            {
                string sql_to_match = @"select 1 -- '
GO
''
GO
";
                var expected_scrubbed = new [] { @"select 1 -- '
" , @"
''
"};
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray(); 
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);

            }

            [Observation]
            public void should_not_replace_on_go_with_preceeding_line_having_comment_inside_string_literal()
            {
                string sql_to_match = @"select ' 1 --
GO
'";
                var expected_scrubbed = new [] { @"select ' 1 --
GO
'" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray(); 
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);

            }

            [Observation]
            public void should_not_replace_on_g()
            {
                const string sql_to_match = @" G
";         
                var expected_scrubbed = new [] { @" G
" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_o()
            {
                const string sql_to_match = @" O
";
                var expected_scrubbed = new [] { @" O
" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_when_go_is_the_last_part_of_the_last_word_on_a_line()
            {
                string sql_to_match = words_to_check + @"GO
";
                var expected_scrubbed =  new [] { words_to_check + @"GO
" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_starting_line()
            {
                string sql_to_match = @"--GO
";
                var expected_scrubbed = new [] { @"--GO
"};
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_and_space_starting_line()
            {
                string sql_to_match = @"-- GO
";
                var expected_scrubbed = new [] { @"-- GO
" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_and_space_starting_line_and_words_after_go()
            {
                string sql_to_match = @"-- GO " + words_to_check + @"
";
                var expected_scrubbed = new [] { @"-- GO " + words_to_check + @"
" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_and_space_starting_line_and_symbols_after_go()
            {
                string sql_to_match = @"-- GO " + symbols_to_check + @"
";
                var expected_scrubbed = new [] { @"-- GO " + symbols_to_check + @"
" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_and_tab_starting_line()
            {
                string sql_to_match = @"--" + string.Format("\t") + @"GO
";
                var expected_scrubbed = new [] { @"--" + string.Format("\t") + @"GO
" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_and_tab_starting_line_and_words_after_go()
            {
                string sql_to_match = @"--" + string.Format("\t") + @"GO " + words_to_check + @"
";
                var expected_scrubbed = new [] { @"--" + string.Format("\t") + @"GO " + words_to_check + @"
"};
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_and_tab_starting_line_and_symbols_after_go()
            {
                string sql_to_match = @"--" + string.Format("\t") + @"GO " + symbols_to_check + @"
";
                var expected_scrubbed = new [] { @"--" + string.Format("\t") + @"GO " + symbols_to_check + @"
" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_starting_line_with_words_before_go()
            {
                string sql_to_match = @"-- " + words_to_check + @" GO
";
                var expected_scrubbed = new [] { @"-- " + words_to_check + @" GO
" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_when_between_tick_marks()
            {
                const string sql_to_match = @"' GO
            '";
                var expected_scrubbed = new [] { @"' GO
            '" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_when_between_tick_marks_with_symbols_and_words_before_ending_on_same_line()
            {
                string sql_to_match = @"' " + symbols_to_check.Replace("'", string.Empty) + words_to_check + @" GO'";
                var expected_scrubbed = new [] { @"' " + symbols_to_check.Replace("'", string.Empty) + words_to_check + @" GO'" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_when_between_tick_marks_with_symbols_and_words_before()
            {
                string sql_to_match = @"' " + symbols_to_check.Replace("'",string.Empty) + words_to_check + @" GO
            '";
                var expected_scrubbed = new [] { @"' " + symbols_to_check.Replace("'", string.Empty) + words_to_check + @" GO
            '" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_when_between_tick_marks_with_symbols_and_words_after()
            {
                string sql_to_match = @"' GO
            " + symbols_to_check.Replace("'", string.Empty) + words_to_check + @"'";
                var expected_scrubbed = new [] { @"' GO
            " + symbols_to_check.Replace("'", string.Empty) + words_to_check + @"'" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_starting_line_with_symbols_before_go()
            {
                string sql_to_match = @"--" + symbols_to_check + @" GO
";
                var expected_scrubbed = new [] { @"--" + symbols_to_check + @" GO
" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_starting_line_with_words_and_symbols_before_go()
            {
                string sql_to_match = @"--" + symbols_to_check + words_to_check + @" GO
";
                var expected_scrubbed = new [] { @"--" + symbols_to_check + words_to_check + @" GO
" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_inside_of_comments()
            {
                string sql_to_match = @"/* GO */";
                var expected_scrubbed = new [] { @"/* GO */" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_inside_of_comments_with_a_line_break()
            {
                string sql_to_match = @"/* GO 
*/";
                var expected_scrubbed = new [] { @"/* GO 
*/" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_inside_of_nested_comments_with_a_line_break()
            {
                string sql_to_match = @"/* /* */ GO 
*/";
                var expected_scrubbed = new [] { @"/* /* */ GO 
*/" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_inside_of_comments_with_words_before()
            {
                string sql_to_match =
                    @"/* 
" + words_to_check + @" GO

*/";
                var expected_scrubbed = new [] {
                    @"/* 
" + words_to_check + @" GO

*/" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_inside_of_comments_with_words_before_on_a_different_line()
            {
                string sql_to_match =
                    @"/* 
" + words_to_check + @" 
GO

*/";
                var expected_scrubbed = new [] {
                    @"/* 
" + words_to_check + @" 
GO

*/" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_inside_of_comments_with_words_before_and_after_on_different_lines()
            {
                string sql_to_match =
                    @"/* 
" + words_to_check + @" 
GO

" + words_to_check + @"
*/";
                var expected_scrubbed = new [] {
                    @"/* 
" + words_to_check + @" 
GO

" + words_to_check + @"
*/" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_inside_of_comments_with_symbols_after_on_different_lines()
            {
                string sql_to_match =
                    @"/* 
GO

" + symbols_to_check + @" 
*/";
                var expected_scrubbed = new [] {
                    @"/* 
GO

" + symbols_to_check + @" 
*/" };
                Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
                CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

        }

		[Concern(typeof(StatementSplitter))]
		public class when_replacing_plsql_statements_with_the_statement_splitters_match_evaluator : concern_for_StatementSplitter
		{
			private because b = () =>
			{
                splitter = new DefaultStatementSplitter(plsql_separator_regex_string);
			};

			[Observation]
			public void should_replace_on_full_statement_without_issue()
			{
				string sql_to_match = SplitterContext.FullSplitter.plsql_statement;
				Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
				CollectionAssert.AreEqual(SplitterContext.FullSplitter.plsql_statement_scrubbed, sql_statement_scrubbed);
			}

			[Observation]
			public void should_replace_on_semicolon_on_its_own_line()
			{
				const string sql_to_match = @"SQL1 
;
SQL2";
				var expected_scrubbed =  new [] { @"SQL1 
 " , @" 
SQL2" };
				Console.WriteLine(sql_to_match);
				var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
				CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
			}

			[Observation]
			public void should_not_replace_on_semicolon_inside_of_comments()
			{
				string sql_to_match = @"/* ; */";
				var expected_scrubbed =  new [] { @"/* ; */" };
				Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
				CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
			}

			[Observation]
			public void should_not_replace_on_semicolon_at_end_of_line()
			{
				string sql_to_match = @"SQL1;";
				var expected_scrubbed = new [] { @"SQL1;" };
				Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
				CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
			}

			[Observation]
			public void should_not_replace_on_assigning_values_to_variables()
			{
				string sql_to_match = @"tmpSql := 'DROP SEQUENCE mutatieStockID';
EXECUTE IMMEDIATE tmpSql; ";
				var expected_scrubbed = new [] { @"tmpSql := 'DROP SEQUENCE mutatieStockID';
EXECUTE IMMEDIATE tmpSql; " };
				Console.WriteLine(sql_to_match);
                var sql_statement_scrubbed = splitter.split(sql_to_match).ToArray();
				CollectionAssert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
			}
		}
    }
}