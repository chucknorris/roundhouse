using System;
using System.Collections.Generic;

namespace roundhouse.databases.mysql.parser
{
    /// <summary>
    /// Provides a scanner that will break a string of MySQL into tokens.
    /// </summary>
    class Scanner
    {
        /// <summary>
        /// Keyword indicating the delimiter is being re-defined
        /// </summary>
        private const string DELIMITER_DECLARE = "delimiter";

        /// <summary>
        /// Single line comment
        /// </summary>
        private const string SINGLE_LINE_COMMENT_DASHED = "--";

        /// <summary>
        /// Inline comment
        /// </summary>
        private const string SINGLE_LINE_COMMENT_HASH = "#";

        /// <summary>
        /// Beginning of a multi-line comment
        /// </summary>
        private const string MULTI_LINE_COMMENT_START = "/*";

        /// <summary>
        /// End of a multi-line comment
        /// </summary>
        private const string MULTI_LINE_COMMENT_CLOSE = "*/";

        /// <summary>
        /// Quotation mark
        /// </summary>
        private const char QUOTE = '`';

        /// <summary>
        /// ANSI quotation mark
        /// </summary>
        private const char ANSI_QUOTE = '\"';

        /// <summary>
        /// Default statement delimiter
        /// </summary>
        private const string DEFAULT_DELIMETER = ";";

        /// <summary>
        /// The MySQL script to scan
        /// </summary>
        private string script;

        /// <summary>
        /// The current statement delimiter
        /// </summary>
        private string delimiter = DEFAULT_DELIMETER;

        /// <summary>
        /// Flag indicating ANSI style quotes will be honored by MySQL
        /// </summary>
        private bool ansiQuotes;

        /// <summary>
        /// Start position in the script for the current token
        /// </summary>
        private int start = 0;

        /// <summary>
        /// Current position in the script
        /// </summary>
        private int current = 0;

        /// <summary>
        /// Current line in the script
        /// </summary>
        private int line = 1;

        /// <summary>
        /// List of tokens in the script
        /// </summary>
        private List<Token> tokens = new List<Token>();

        /// <summary>
        /// Creates a new scanner and sets its MySQL script
        /// </summary>
        /// <param name="script">the MySQL script to parse</script>
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

        /// <summary>
        /// Scans the MySQL script and returns a List of Token instances
        /// </summary>
        /// <returns>List of Token</return>
        public List<Token> Scan() 
        {

            while (!IsAtEnd()) {
                
                start = current;
                ScanToken();
            }

            EndOfFile();
            return tokens;
        }

        private void ScanToken() 
        {

            char c = Advance();

            // we're giving delimiters precedence of other tokens
            if (delimiter.Length == 1 && c == delimiter[0]) {
                SingleCharacterDelimiter();
                return;
            } else if (delimiter.Length == 2 && c == delimiter[0] && Peek() == delimiter[1]) {
                TwoCharacterDelimiter();
                return;
            } else if (delimiter.Length > 2 && c == delimiter[0] && Peek() == delimiter[1]) {
                if(MultiCharacterDelimiter()) {
                    return;
                }
            }

            switch (c)
            {
                // whitespace we can ignore, but delimits tokens
                case '\t':
                case ' ':
                    Whitespace();
                    break;

                case '\r':
                    if (Peek() == '\n') {
                        Advance();  // we are discarding /r
                        EndOfLine();
                        break;
                    } else {
                        goto case '\n';
                    }

                case '\n':
                    EndOfLine();
                    break;

                case '"':
                    if (ansiQuotes) {
                        AnsiQuoted();
                        break;
                    } else {
                        goto case '`';
                    }

                case '`':
                    Quoted();
                    break;

                case '#':
                    Comment();
                    break;
                
                case '-':
                    if (Match('-')) {
                        Advance();
                        Comment();
                        break;
                    } else {
                        goto case '/';
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
            while (!(Peek() == '\r' && PeekPeek() == '\n') && Peek() != '\n' && !IsAtEnd()) {
                Advance();
            }

            AddToken(Token.Type.Comment);
        }

        private void SingleCharacterDelimiter()
        {
            AddToken(Token.Type.Delimiter);
        }

        private void TwoCharacterDelimiter()
        {
            Advance();  // consume second character
            AddToken(Token.Type.Delimiter);
        }

        private bool MultiCharacterDelimiter() 
        {
            if (start + delimiter.Length <= script.Length) {

                string possibleDelimiter = script.Substring(start, delimiter.Length);
                if (possibleDelimiter.Equals(delimiter)) {
                    current = start + delimiter.Length;  // consume the rest of the delimiter
                    AddToken(Token.Type.Delimiter);
                    return true;
                }
            }

            return false;
        }

        private void Whitespace()
        {
            while (!IsAtEnd() && Peek() != '\n' && Char.IsWhiteSpace(Peek())) {
                Advance();
            }

            AddToken(Token.Type.Whitespace);
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
            line++;
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

        private void DelimiterDeclaration() {

            // add our declaration keyword
            int end = current - start;
            string value = script.Substring(start, end);
            tokens.Add(new Token(Token.Type.DelimiterDeclare, value, line, start));

            // move our start forward for next scan
            start = current;
            char c = Advance();

            // consume any whitespace
            Whitespace();

            if (IsAtEnd()) {
                // no delimiter provided
                throw new Exception("Delimiter keyword used but no delimiter was provided");
            }

            // consume the new delimiter
            while (Peek() != '\n' && !IsAtEnd()) {
                Advance();
            }

            // add the new delimiter
            end = current - start;
            value = script.Substring(start, end).Trim();
            tokens.Add(new Token(Token.Type.Delimiter, value, line, start));

            // set our new delimiter
            delimiter = value;
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

            // delimiter redeclaration
            string value = script.Substring(start, end);
            if (value.ToLower().Equals(DELIMITER_DECLARE)) {
                DelimiterDeclaration();
                return;
            }

            // typical token addition
            tokens.Add(new Token(type, value, line, start));
        }

        private void AddEmptyToken(Token.Type type)
        {
            tokens.Add(new Token(type, null, line, start));
        }
    }
}