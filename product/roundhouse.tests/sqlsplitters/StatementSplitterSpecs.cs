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
            protected static string tsql_separator_regex_string;
            protected static string access_sql_separator_regex_string;
            protected static string plsql_separator_regex_string;

            private context c = () =>
                                    {
                                        Database database = new SqlServerDatabase();
                                        tsql_separator_regex_string = database.sql_statement_separator_regex_pattern;
                                        database = new AccessDatabase();
                                        access_sql_separator_regex_string = database.sql_statement_separator_regex_pattern;
                                        database = new OracleDatabase();
                                        plsql_separator_regex_string = database.sql_statement_separator_regex_pattern;
                                    };
        }

        [Concern(typeof(StatementSplitter))]
        public class when_replacing_tsql_statements_with_the_statement_splitters_match_evaluator : concern_for_StatementSplitter
        {
            protected static Regex script_regex_replace;
            protected static string batch_terminator_replacement_string = StatementSplitter.batch_terminator_replacement_string;
            protected static string regex_split_string = StatementSplitter.regex_split_string;
            protected static string symbols_to_check = "`~!@#$%^&*()-_+=,.;:'\"[]\\/?<>";
            protected static string words_to_check = "abcdefghijklmnopqrstuvwzyz0123456789 ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            private because b = () =>
                                    {
                                        script_regex_replace = new Regex(tsql_separator_regex_string, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    };

            [Observation]
            public void should_replace_on_full_statement_without_issue()
            {
                string sql_to_match = SplitterContext.FullSplitter.tsql_statement;
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(SplitterContext.FullSplitter.tsql_statement_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_space()
            {
                const string sql_to_match = @" GO ";
                string expected_scrubbed = @" " + batch_terminator_replacement_string + @" ";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_tab()
            {
                string sql_to_match = @" GO" + string.Format("\t");
                string expected_scrubbed = @" " + batch_terminator_replacement_string + string.Format("\t");
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_new_line()
            {
                const string sql_to_match = @" GO
";
                string expected_scrubbed = @" " + batch_terminator_replacement_string + @"
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_on_new_line_after_double_dash_comments()
            {
                const string sql_to_match =
                    @"--
GO
";
                string expected_scrubbed =
                    @"--
" + batch_terminator_replacement_string + @"
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_on_new_line_after_double_dash_comments_and_words()
            {
                string sql_to_match = @"-- " + words_to_check + @"
GO
";
                string expected_scrubbed = @"-- " + words_to_check + @"
" + batch_terminator_replacement_string + @"
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_new_line_after_double_dash_comments_and_symbols()
            {
                string sql_to_match = @"-- " + symbols_to_check + @"
GO
";
                string expected_scrubbed = @"-- " + symbols_to_check + @"
" + batch_terminator_replacement_string + @"
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_on_its_own_line()
            {
                const string sql_to_match = @" 
GO
";
                string expected_scrubbed = @" 
" + batch_terminator_replacement_string + @"
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_no_line_terminator()
            {
                const string sql_to_match = @" GO ";
                string expected_scrubbed = @" " + batch_terminator_replacement_string + @" ";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_words_before()
            {
                string sql_to_match = words_to_check + @" GO
";
                string expected_scrubbed = words_to_check + @" " + batch_terminator_replacement_string + @"
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_symbols_and_words_before()
            {
                string sql_to_match = symbols_to_check + words_to_check + @" GO
";
                string expected_scrubbed = symbols_to_check + words_to_check + @" " + batch_terminator_replacement_string + @"
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_words_and_symbols_before()
            {
                string sql_to_match = words_to_check + symbols_to_check + @" GO
";
                string expected_scrubbed = words_to_check + symbols_to_check + @" " + batch_terminator_replacement_string + @"
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_words_after_on_the_same_line()
            {
                string sql_to_match = @" GO " + words_to_check;
                string expected_scrubbed = @" " + batch_terminator_replacement_string + @" " + words_to_check;
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_words_after_on_the_same_line_including_symbols()
            {
                string sql_to_match = @" GO " + words_to_check + symbols_to_check;
                string expected_scrubbed = @" " + batch_terminator_replacement_string + @" " + words_to_check + symbols_to_check;
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_words_before_and_after_on_the_same_line()
            {
                string sql_to_match = words_to_check + @" GO " + words_to_check;
                string expected_scrubbed = words_to_check + @" " + batch_terminator_replacement_string + @" " + words_to_check;
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_words_before_and_after_on_the_same_line_including_symbols()
            {
                string sql_to_match = words_to_check + symbols_to_check.Replace("'","").Replace("\"","") + " GO BOB" + symbols_to_check;
                string expected_scrubbed = words_to_check + symbols_to_check.Replace("'", "").Replace("\"", "") + " " + batch_terminator_replacement_string + " BOB" + symbols_to_check;
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_after_double_dash_comment_with_single_quote_and_single_quote_after_go()
            {
                string sql_to_match = words_to_check + @" -- '
GO
select ''
go";
                string expected_scrubbed = words_to_check + @" -- '
" + batch_terminator_replacement_string + @"
select ''
" + batch_terminator_replacement_string;
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_replace_on_go_with_comment_after()
            {
                string sql_to_match = " GO -- comment";
                string expected_scrubbed = " " + batch_terminator_replacement_string + " -- comment";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_g()
            {
                const string sql_to_match = @" G
";         
                const string expected_scrubbed = @" G
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_o()
            {
                const string sql_to_match = @" O
";
                const string expected_scrubbed = @" O
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_when_go_is_the_last_part_of_the_last_word_on_a_line()
            {
                string sql_to_match = words_to_check + @"GO
";
                string expected_scrubbed = words_to_check + @"GO
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_starting_line()
            {
                string sql_to_match = @"--GO
";
                string expected_scrubbed = @"--GO
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_and_space_starting_line()
            {
                string sql_to_match = @"-- GO
";
                string expected_scrubbed = @"-- GO
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_and_space_starting_line_and_words_after_go()
            {
                string sql_to_match = @"-- GO " + words_to_check + @"
";
                string expected_scrubbed = @"-- GO " + words_to_check + @"
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_and_space_starting_line_and_symbols_after_go()
            {
                string sql_to_match = @"-- GO " + symbols_to_check + @"
";
                string expected_scrubbed = @"-- GO " + symbols_to_check + @"
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_and_tab_starting_line()
            {
                string sql_to_match = @"--" + string.Format("\t") + @"GO
";
                string expected_scrubbed = @"--" + string.Format("\t") + @"GO
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_and_tab_starting_line_and_words_after_go()
            {
                string sql_to_match = @"--" + string.Format("\t") + @"GO " + words_to_check + @"
";
                string expected_scrubbed = @"--" + string.Format("\t") + @"GO " + words_to_check + @"
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_and_tab_starting_line_and_symbols_after_go()
            {
                string sql_to_match = @"--" + string.Format("\t") + @"GO " + symbols_to_check + @"
";
                string expected_scrubbed = @"--" + string.Format("\t") + @"GO " + symbols_to_check + @"
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_starting_line_with_words_before_go()
            {
                string sql_to_match = @"-- " + words_to_check + @" GO
";
                string expected_scrubbed = @"-- " + words_to_check + @" GO
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_when_between_tick_marks()
            {
                const string sql_to_match = @"' GO
            '";
                const string expected_scrubbed = @"' GO
            '";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_when_between_tick_marks_with_symbols_and_words_before_ending_on_same_line()
            {
                string sql_to_match = @"' " + symbols_to_check.Replace("'", string.Empty) + words_to_check + @" GO'";
                string expected_scrubbed = @"' " + symbols_to_check.Replace("'", string.Empty) + words_to_check + @" GO'";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_when_between_tick_marks_with_symbols_and_words_before()
            {
                string sql_to_match = @"' " + symbols_to_check.Replace("'",string.Empty) + words_to_check + @" GO
            '";
                string expected_scrubbed = @"' " + symbols_to_check.Replace("'", string.Empty) + words_to_check + @" GO
            '";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_when_between_tick_marks_with_symbols_and_words_after()
            {
                string sql_to_match = @"' GO
            " + symbols_to_check.Replace("'", string.Empty) + words_to_check + @"'";
                string expected_scrubbed = @"' GO
            " + symbols_to_check.Replace("'", string.Empty) + words_to_check + @"'";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_starting_line_with_symbols_before_go()
            {
                string sql_to_match = @"--" + symbols_to_check + @" GO
";
                string expected_scrubbed = @"--" + symbols_to_check + @" GO
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_with_double_dash_comment_starting_line_with_words_and_symbols_before_go()
            {
                string sql_to_match = @"--" + symbols_to_check + words_to_check + @" GO
";
                string expected_scrubbed = @"--" + symbols_to_check + words_to_check + @" GO
";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_inside_of_comments()
            {
                string sql_to_match = @"/* GO */";
                string expected_scrubbed = @"/* GO */";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_inside_of_comments_with_a_line_break()
            {
                string sql_to_match = @"/* GO 
*/";
                string expected_scrubbed = @"/* GO 
*/";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_inside_of_comments_with_words_before()
            {
                string sql_to_match =
                    @"/* 
" + words_to_check + @" GO

*/";
                string expected_scrubbed =
                    @"/* 
" + words_to_check + @" GO

*/";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_inside_of_comments_with_words_before_on_a_different_line()
            {
                string sql_to_match =
                    @"/* 
" + words_to_check + @" 
GO

*/";
                string expected_scrubbed =
                    @"/* 
" + words_to_check + @" 
GO

*/";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
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
                string expected_scrubbed =
                    @"/* 
" + words_to_check + @" 
GO

" + words_to_check + @"
*/";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

            [Observation]
            public void should_not_replace_on_go_inside_of_comments_with_symbols_after_on_different_lines()
            {
                string sql_to_match =
                    @"/* 
GO

" + symbols_to_check + @" 
*/";
                string expected_scrubbed =
                    @"/* 
GO

" + symbols_to_check + @" 
*/";
                Console.WriteLine(sql_to_match);
                string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
                Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
            }

        }

		[Concern(typeof(StatementSplitter))]
		public class when_replacing_plsql_statements_with_the_statement_splitters_match_evaluator : concern_for_StatementSplitter
		{
			protected static Regex script_regex_replace;
			protected static string batch_terminator_replacement_string = StatementSplitter.batch_terminator_replacement_string;
			protected static string regex_split_string = StatementSplitter.regex_split_string;
			
			private because b = () =>
			{
				script_regex_replace = new Regex(plsql_separator_regex_string, RegexOptions.IgnoreCase | RegexOptions.Multiline);
			};

			[Observation]
			public void should_replace_on_full_statement_without_issue()
			{
				string sql_to_match = SplitterContext.FullSplitter.plsql_statement;
				Console.WriteLine(sql_to_match);
				string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
				Assert.AreEqual(SplitterContext.FullSplitter.plsql_statement_scrubbed, sql_statement_scrubbed);
			}

			[Observation]
			public void should_replace_on_semicolon_on_its_own_line()
			{
				const string sql_to_match = @"SQL1 
;
SQL2";
				string expected_scrubbed = @"SQL1 
" + batch_terminator_replacement_string + @"
SQL2";
				Console.WriteLine(sql_to_match);
				string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
				Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
			}

			[Observation]
			public void should_not_replace_on_semicolon_inside_of_comments()
			{
				string sql_to_match = @"/* ; */";
				string expected_scrubbed = @"/* ; */";
				Console.WriteLine(sql_to_match);
				string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
				Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
			}

			[Observation]
			public void should_not_replace_on_semicolon_at_end_of_line()
			{
				string sql_to_match = @"SQL1;";
				string expected_scrubbed = @"SQL1;";
				Console.WriteLine(sql_to_match);
				string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
				Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
			}

			[Observation]
			public void should_not_replace_on_assigning_values_to_variables()
			{
				string sql_to_match = @"tmpSql := 'DROP SEQUENCE mutatieStockID';
EXECUTE IMMEDIATE tmpSql; ";
				string expected_scrubbed = @"tmpSql := 'DROP SEQUENCE mutatieStockID';
EXECUTE IMMEDIATE tmpSql; ";
				Console.WriteLine(sql_to_match);
				string sql_statement_scrubbed = script_regex_replace.Replace(sql_to_match, match => StatementSplitter.evaluate_and_replace_batch_split_items(match, script_regex_replace));
				Assert.AreEqual(expected_scrubbed, sql_statement_scrubbed);
			}
		}
    }
}

