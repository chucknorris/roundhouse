namespace roundhouse.databases.mysql.parser
{
    public class ParsedStatement
    {
        public enum Type { Sql, Delimiter };

        private Type type = Type.Sql;
        private string value = null;
        private string delimiter = null;

        public ParsedStatement(string value, string delimiter)
        {
            this.type = Type.Sql;
            this.value = value;
            this.delimiter = delimiter;
        }
        public ParsedStatement(Type type, string value, string delimiter)
        {
            this.type = type;
            this.value = value;
            this.delimiter = delimiter;
        }

        public ParsedStatement.Type StatementType
        {
            get
            {
                return this.type;
            }
        }

        public string Value
        {
            get
            {
                return this.value;
            }
        }

        public string Delimiter
        {
            get
            {
                return this.delimiter;
            }
        }
    }
}