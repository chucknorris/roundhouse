using System;

namespace roundhouse.databases.mysql 
{

    class MySqlTokenizer
    {

        private const string DEFINER = "definer";
        private const string SINGLE_LINE_COMMENT_DASHED = "--";
        private const string SINGLE_LINE_COMMENT_HASH = "#";
        private const string MULTI_LINE_COMMENT_START = "/*";
        private const string MULTI_LINE_COMMENT_CLOSE = "*/";
        private const char QUOTE = '`';
        private const char ANSI_QUOTE = '\"';
        private string script;
        private string delimiter = ";";
        private bool ansiQuotes;
        private int start = 0;
        private int current = 0;
        private int line = 1;

        public MySqlTokenizer(string script)
        {
            this.script = script;
        }

        public string Delimiter
        {
            get 
            {
                return this.delimiter;
            }
            set 
            {
                this.delimiter = value;
            }
        }

        public bool AnsiQuotes
        {
            get
            {
                return this.ansiQuotes;
            }
            set 
            {
                this.ansiQuotes = value;
            }
        }

        public void Scan() 
        {

            while (!IsAtEnd()) {
                
                start = current;
                ScanToken();
            }
        }

        public void ScanToken() 
        {

            char c = Advance();

            switch (c)
            {

                default:
                    General();
                    break;
            }
        }

        private void General()
        {
            while (!Char.IsWhiteSpace(Peek())) {
                Advance();
            }

            // add token here!
        }

        private char Peek() 
        {
            if (IsAtEnd()) {
                return '\0';
            }

            return script[current];
        }

        private char Advance() 
        {

            current++;
            return script[current];
        }

        private bool IsAtEnd()
        {
            bool value = false;

            if (current >= script.Length) {
                value = true;
            }

            return value;
        }

        private bool IsQuote(char c) 
        {
            bool value = false;

            if (c == QUOTE) {
                value = true;
            }
            
            return value;
        }

        private bool IsAnsiQuote(char c) 
        {
            bool value = false;

            if(c == ANSI_QUOTE) {
                value = true;
            }

            return value;
        }
    }

    class Token
    {
        public enum Type { DelimiterDeclaration, Delimiter, Comment, Text, QuotedText };

        private Type type = Type.Text;
        private string value = null;
        private int line = 0;
        private int column = 0;

        public Token(string value, int line, int column) : this (Type.Text, value, line, column) {
            
        }

        public Token(Type type, string value, int line, int column) : this(type, value, line, column, false) {

        }

        public Token(Type type, string value, int line, int column, bool quote) 
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