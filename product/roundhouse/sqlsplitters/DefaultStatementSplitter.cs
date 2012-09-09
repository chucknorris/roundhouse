namespace roundhouse.sqlsplitters
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using infrastructure.extensions;

    public class DefaultStatementSplitter : StatementSplitter
    {
        public DefaultStatementSplitter(string sql_statement_separator_regex_pattern)
        {
            this.sql_statement_separator_regex_pattern = sql_statement_separator_regex_pattern;
        }

        private string sql_statement_separator_regex_pattern;

        private static string batch_terminator_replacement_string = @" |{[_REMOVE_]}| ";
        private static string regex_split_string = @"\|\{\[_REMOVE_\]\}\|";

        public IEnumerable<string> split(string sql_to_run)
        {
            IList<string> return_sql_list = new List<string>();

            Regex regex_replace = new Regex(sql_statement_separator_regex_pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            string sql_statement_scrubbed = regex_replace.Replace(sql_to_run, match => evaluate_and_replace_batch_split_items(match, regex_replace));

            foreach (string sql_statement in Regex.Split(sql_statement_scrubbed, regex_split_string, RegexOptions.IgnoreCase | RegexOptions.Multiline))
            {
                if (script_has_text_to_run(sql_statement, sql_statement_separator_regex_pattern))
                {
                    return_sql_list.Add(sql_statement);
                }
            }

            return return_sql_list;
        }

        private static string evaluate_and_replace_batch_split_items(Match matched_item, Regex regex)
        {
            if (matched_item.Groups["BATCHSPLITTER"].Success)
            {
                return matched_item.Groups["KEEP1"].Value + batch_terminator_replacement_string + matched_item.Groups["KEEP2"].Value;
            }
            else
            {
                return matched_item.Groups["KEEP1"].Value + matched_item.Groups["KEEP2"].Value;
            }
        }

        private static bool script_has_text_to_run(string sql_statement, string sql_statement_separator_regex_pattern)
        {
            sql_statement = Regex.Replace(sql_statement, regex_split_string, string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            return !string.IsNullOrEmpty(sql_statement.to_lower().Replace(System.Environment.NewLine, string.Empty).Replace(" ", string.Empty));
        }
    }
}