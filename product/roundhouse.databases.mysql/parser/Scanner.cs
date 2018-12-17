using System;
using System.Collections.Generic;

namespace roundhouse.databases.mysql.parser
{
    class Scanner
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
        private List<Token> tokens = new List<Token>();

        public Scanner(string script)
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

        public List<Token> Scan() 
        {

            while (!IsAtEnd()) {
                
                start = current;
                ScanToken();
            }

            EndOfFile();
            return tokens;
        }

        public void ScanToken() 
        {

            char c = Advance();

            switch (c)
            {
                // whitespace we can ignore, but delimits tokens
                case '\t':
                case ' ':
                case '\r':
                case '\n':
                    break;

                case '"':
                    AnsiQuoted();
                    break;

                case '`':
                    Quoted();
                    break;

                case '#':
                    Comment();
                    break;

                case ';':
                    DefaultDelimiter();
                    break;
                
                case '-':
                    if (Match('-')) {
                        Advance();
                        Comment();
                        break;
                    } else {
                        General();
                        break;
                    }
                
                case '/':
                    if (Match('*')) {
                        Advance();
                        MultiLineComment();
                        break;
                    } else {
                        General();
                        break;
                    }

                default:
                    General();
                    break;
            }
        }

        private void MultiLineComment()
        {
            while (Peek() != '*' && PeekPeek() != '/') {

                if (Peek() == '\n') {
                    line++;
                }

                Advance();

                if (IsAtEnd()) {
                    // unterminated comment
                    throw new Exception("Unterminated comment on line " + line);
                }
            }

            Advance(); // consume *
            Advance(); // consume /

            AddToken(Token.Type.Comment);
        }

        private void Comment()
        {
            while (Peek() != '\n' && !IsAtEnd()) {
                Advance();
            }

            AddToken(Token.Type.Comment);
        }

        private void DefaultDelimiter()
        {
            if (delimiter.Length == 1 && delimiter[0] == ';') {
                AddToken(Token.Type.Delimiter);
            }
        }

        private void Quoted()
        {
            while (!IsAtEnd() && !IsQuote(Peek())) {
                
                if (Peek() == '\n') {
                    line++;
                }

                Advance();

                if (IsAtEnd()) {
                    // unterminated string
                    throw new Exception("Unterminated quoted value on line " + line);
                }
            }

            Advance(); // closing quote
            AddToken(Token.Type.Quote);
        }

        private void AnsiQuoted()
        {
            while (!IsAtEnd() && !IsAnsiQuote(Peek())) {
                
                if (Peek() == '\n') {
                    line++;
                }

                Advance();

                if (IsAtEnd()) {
                    // unterminated string
                    throw new Exception("Unterminated quoted value on line " + line);
                }
            }

            Advance(); // closing quote
            AddToken(Token.Type.AnsiQuote);
        }

        private void General()
        {
            while (!IsAtEnd() && Char.IsLetterOrDigit(Peek())) {
                Advance();
            }

            AddToken(Token.Type.Text);
        }

        private void EndOfFile()
        {
            AddEmptyToken(Token.Type.EndOfFile);
        }

        private void EndOfLine()
        {
            AddEmptyToken(Token.Type.EndOfLine);
        }

        private bool Match(char expected)
        {

            if (IsAtEnd()) {
                return false;
            }

            if(script[current] != expected) {
                return false;
            }

            current++;
            return true;
        }

        private char Peek() 
        {
            if (IsAtEnd()) {
                return '\0';
            }

            return script[current];
        }

        private char PeekPeek() 
        {
            if (IsAtEnd() || (current + 1 >= script.Length)) {
                return '\0';
            }

            return script[current + 1];
        }

        private char Advance() 
        {

            current++;
            return script[current - 1];
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

        private void AddToken(Token.Type type)
        {
            int end = current - start;
            if (end < 0) {
                end = 0;
            }

            if(current + end < 1) {
                return;
            }

            Console.Out.WriteLine("TOKEN [" + type + ", " + current + ", " + end + "]: " + script.Substring(start, end));
            tokens.Add(new Token(type, script.Substring(start, end), line, start));
        }

        private void AddEmptyToken(Token.Type type)
        {
            Console.Out.WriteLine("TOKEN [" + type + "]: ");
            tokens.Add(new Token(type, null, line, start));
        }
    }
}