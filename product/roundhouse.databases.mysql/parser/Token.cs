namespace roundhouse.databases.mysql.parser
{
    /// <summary>
    /// Models a Token of a MySQL script
    /// </summary>
    class Token
    {
        /// <summary>
        /// Enumeration of MySQL token types
        /// </summary>
        public enum Type { 
            DelimiterDeclare,  // statement delimiter declaration
            Delimiter,         // statement delimiter
            Comment,           // comment
            Text,              // SQL script text
            Quote,             // quoted text
            AnsiQuote,         // ANSI quoted text
            Whitespace,        // whitespace
            EndOfLine,         // end of line
            EndOfFile          // end of file
        };

        /// <summary>
        /// Type of token
        /// </summary>
        private readonly Type type = Type.Text;

        /// <summary>
        /// Value of the token
        /// </summary>
        private readonly string value;

        /// <summary>
        /// Line on which the token was found
        /// </summary>
        private readonly int line;

        /// <summary>
        /// Column on which the token was found
        /// </summary>
        private readonly int column;

        /// <summary>
        /// Creates a new token of the type Text and sets its fields
        /// </summary>
        /// <param name="value">Value of the token</param>
        /// <param name="line">Line where the token was found</param>
        /// <param name="column">Column where the token was found</param>
        public Token(string value, int line, int column) : this (Type.Text, value, line, column) {
            
        }

        /// <summary>
        /// Creates a new token and sets its fields
        /// </summary>
        /// <param name="type">Type of the token</param>
        /// <param name="value">Value of the token</param>
        /// <param name="line">Line where the token was found</param>
        /// <param name="column">Column where the token was found</param>
        public Token(Type type, string value, int line, int column) 
        {
            this.type = type;
            this.value = value;
            this.line = line;
            this.column = column;
        }

        public Token.Type TokenType
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

        public int  Line
        {
            get
            {
                return this.line;
            }
        }

        public int Column
        {
            get
            {
                return this.column;
            }
        }
    }
}