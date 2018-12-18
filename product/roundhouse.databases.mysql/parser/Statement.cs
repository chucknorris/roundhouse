namespace roundhouse.databases.mysql.parser
{
    /// <summary>
    /// Models a statemetn of a MySQL script
    /// </summary>
    public class ParsedStatement
    {
        /// <summary>
        /// Enumeration of MySQL statement types
        /// </summary>
        public enum Type { 
            Sql,       // SQL statement
            Delimiter  // delimiter declaration
        };

        /// <summary>
        /// Type of statement
        /// </summary>
        private Type type = Type.Sql;

        /// <summary>
        /// Value of the statement
        /// </summary>
        private string value = null;

        /// <summary>
        /// Delimiter for the statement
        /// </summary>
        private string delimiter = null;

        /// <summary>
        /// Creates a new statement of type Sql and sets its fields
        /// </summary>
        /// <param name="value">Value of the statement</param>
        /// <param name="delimiter">Delimiter for the statement</param>
        public ParsedStatement(string value, string delimiter)
        {
            this.type = Type.Sql;
            this.value = value;
            this.delimiter = delimiter;
        }

        /// <summary>
        /// Creates a new statement and sets its fields
        /// </summary>
        /// <param name="type">Type of statement</param>
        /// <param name="value">Value of the statement</param>
        /// <param name="delimiter">Delimiter for the statement</param>
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