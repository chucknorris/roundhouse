namespace roundhouse.databases.mysql.parser
{
    class Token
    {
        public enum Type { DelimiterDeclare, Delimiter, Comment, Text, Quote, AnsiQuote, Whitespace, EndOfLine, EndOfFile };

        private Type type = Type.Text;
        private string value = null;
        private int line = 0;
        private int column = 0;

        public Token(string value, int line, int column) : this (Type.Text, value, line, column) {
            
        }

        public Token(Type type, string value, int line, int column) 
        {
            this.type = type;
            this.value = value;
            this.line = line;
            this.column = column;
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