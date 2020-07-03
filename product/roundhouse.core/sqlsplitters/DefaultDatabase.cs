namespace roundhouse.sqlsplitters
{
    public static class DefaultDatabase
    {
        public const string default_sql_statement_separator_regex_pattern = @"(?<KEEP1>^(?:[\s\t])*(?:-{2}).*$)|(?<KEEP1>/{1}\*{1}[\S\s]*?\*{1}/{1})|(?<KEEP1>'{1}(?:[^']|\n[^'])*?'{1})|(?<KEEP1>^|\s)(?<BATCHSPLITTER>\;)(?<KEEP2>\s|$)";
    }
}