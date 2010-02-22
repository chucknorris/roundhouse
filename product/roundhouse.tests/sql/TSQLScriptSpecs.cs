namespace roundhouse.tests.sql
{
    using System;
    using System.Text.RegularExpressions;
    using bdddoc.core;
    using developwithpassion.bdd.contexts;
    using developwithpassion.bdd.mbunit.standard;
    using developwithpassion.bdd.mbunit.standard.observations;
    using MbUnit.Framework;
    using roundhouse.sql;

    public class TSQLScriptSpecs
    {
        public abstract class concern_for_TSQLScript : observations_for_a_sut_with_a_contract<SqlScript, TSQLScript>
        {
            private context c = () => { };
        }

        [Concern(typeof(TSQLScript))]
        public class when_splitting_statements_on_the_batch_terminator : concern_for_TSQLScript
        {
            protected static Regex script_regex;
            protected static string symbols_to_check = "='.>?<()[];-*";


            private because b = () =>
                                    {
                                        script_regex = new Regex(sut.separator_characters_regex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    };

            [Observation]
            public void should_split_on_go_with_space()
            {
                const string sql_to_match = @" GO ";
                Assert.IsTrue(script_regex.Match(sql_to_match).Success);
            }
            
            [Observation]
            public void should_split_on_go_with_tab()
            {
                string sql_to_match = @"GO" +  string.Format("\t");
                Console.WriteLine(sql_to_match);
                Assert.IsTrue(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_split_on_go_with_new_line()
            {
                const string sql_to_match = @" GO
";
                Assert.IsTrue(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_split_on_go_with_on_new_line_after_double_dash_comments()
            {
                const string sql_to_match = 
@"--
GO
";
                Assert.IsTrue(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_split_on_go_with_on_new_line_after_double_dash_comments_and_words()
            {
                const string sql_to_match =
@"-- BOB
GO
";
                Assert.IsTrue(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_split_on_go_with_on_new_line_after_double_dash_comments_and_symbols()
            {
                string sql_to_match =
@"-- " + symbols_to_check + @"
GO
";
                Assert.IsTrue(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_split_on_go_on_its_own_line()
            {
                const string sql_to_match =
@" 
GO
";
                Assert.IsTrue(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_split_on_go_with_no_line_terminator()
            {
                const string sql_to_match = @" GO";
                Assert.IsTrue(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_split_on_go_with_words_before()
            {
                const string sql_to_match = @"BOB GO
";
                Assert.IsTrue(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_split_on_go_with_symbols_and_words_before()
            {
                string sql_to_match = symbols_to_check + @"BOB GO
";
                Assert.IsTrue(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_split_on_go_with_words_and_symbols_before()
            {
                string sql_to_match = @"BOB" + symbols_to_check + @" GO
";
                Assert.IsTrue(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_g()
            {
                const string sql_to_match = @" G
";
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_o()
            {
                const string sql_to_match = @" O
";
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }
            
            [Observation]
            public void should_not_split_on_go_when_go_is_the_last_part_of_the_last_word_on_a_line()
            {
                const string sql_to_match = @" BOBGO
";
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_go_with_words_after_on_the_same_line()
            {
                const string sql_to_match = @" GO BOB";
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_go_with_words_after_on_the_same_line_including_symbols()
            {
                string sql_to_match = @" GO BOB" + symbols_to_check;
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_go_with_words_before_and_after_on_the_same_line()
            {
                const string sql_to_match = @" BOB GO BOB";
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_go_with_words_before_and_after_on_the_same_line_including_symbols()
            {
                string sql_to_match = @" BOB" + symbols_to_check + " GO BOB" + symbols_to_check;
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }
            
            [Observation]
            public void should_not_split_on_go_with_double_dash_comment_starting_line()
            {
                string sql_to_match = @"--GO
";
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_go_with_double_dash_comment_and_space_starting_line()
            {
                string sql_to_match = @"-- GO
";
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_go_with_double_dash_comment_and_space_starting_line_and_words_after_go()
            {
                string sql_to_match = @"-- GO bob
";
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_go_with_double_dash_comment_and_space_starting_line_and_symbols_after_go()
            {
                string sql_to_match = @"-- GO " + symbols_to_check + @"
";
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_go_with_double_dash_comment_and_tab_starting_line()
            {
                string sql_to_match = @"--" + string.Format("\t") +   @"GO
";
                Console.WriteLine(sql_to_match);
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_go_with_double_dash_comment_and_tab_starting_line_and_words_after_go()
            {
                string sql_to_match = @"--" + string.Format("\t") + @"GO bob
";
                Console.WriteLine(sql_to_match);
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_go_with_double_dash_comment_and_tab_starting_line_and_symbols_after_go()
            {
                string sql_to_match = @"--" + string.Format("\t") + @"GO " + symbols_to_check + @"
";
                Console.WriteLine(sql_to_match);
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_go_with_double_dash_comment_starting_line_with_words_before_go()
            {
                const string sql_to_match = @"-- BOB GO
";
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

//            [Observation]
//            public void should_not_split_on_go_when_between_tick_marks()
//            {
//                const string sql_to_match = @"' GO
//'";
//                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
//            }

            [Observation]
            public void should_not_split_on_go_with_double_dash_comment_starting_line_with_symbols_before_go()
            {
                string sql_to_match = @"--" + symbols_to_check + @" GO
";
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_go_with_double_dash_comment_starting_line_with_words_and_symbols_before_go()
            {
                string sql_to_match = @"--" + symbols_to_check + @" BOB GO
";
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_go_inside_of_comments()
            {
                string sql_to_match = @"/* GO */";
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_go_inside_of_comments_with_a_line_break()
            {
                string sql_to_match = @"/* GO 
*/";
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }
            
            [Observation]
            public void should_not_split_on_go_inside_of_comments_with_words_before()
            {
                string sql_to_match =
                    @"/* 
BOB GO

*/";
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_go_inside_of_comments_with_words_before_on_a_different_line()
            {
                string sql_to_match =
                    @"/* 
BOB 
GO

*/";
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_go_inside_of_comments_with_words_before_and_after_on_different_lines()
            {
                string sql_to_match =
@"/* 
BOB 
GO

adf
*/";
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

            [Observation]
            public void should_not_split_on_go_inside_of_comments_with_symbols_after_on_different_lines()
            {
                string sql_to_match =
@"/* 
GO

" + symbols_to_check + @" 
*/";
                Assert.IsFalse(script_regex.Match(sql_to_match).Success);
            }

        }
    }
}