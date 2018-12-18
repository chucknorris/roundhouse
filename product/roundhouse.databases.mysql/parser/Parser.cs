using System;
using System.Collections.Generic;
using System.Text;
using roundhouse.databases.mysql.parser;

namespace roundhouse.databases.mysql.parser
{

    /// <summary>
    /// Provides a parser that will parse statements out of a string of MySQL SQL.
    /// </summary>
    public class Parser
    {

        /// <summary>
        /// MySQL script to parse
        /// </summary>
        private string script;

        /// <summary>
        /// Scanner to break script into tokens
        /// </summary>
        private Scanner scanner;

        /// <summary>
        /// List of scanned tokens from the script
        /// </summary>
        private List<Token> tokens;

        /// <summary>
        /// List of accumulated statements
        /// </summary>
        private List<ParsedStatement> statements = new List<ParsedStatement>();

        /// <summary>
        /// Flag indicating ANSI style quotes will be honored by MySQL
        /// </summary>
        private bool ansiQuotes;

        /// <summary>
        /// Start position in the token list for the current statement
        /// </summary>
        private int start = 0;

        /// <summary>
        /// Current position in the token list
        /// </summary>
        private int current = 0;

        /// <summary>
        /// Creates a new parser and sets its MySQL script.
        /// </summary>
        /// <param name="script">the MySQL script to parse</param>
        public Parser(string script)
        {
            this.script = script;
            this.scanner = new Scanner(script);
            this.scanner.AnsiQuotes = ansiQuotes;       
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

                if (scanner != null) {
                    scanner.AnsiQuotes = value;
                }
            }
        }

        /// <summary>
        /// Parses the MySQL script and returns a List of ParsedStatement
        /// instances.
        /// </summary>
        /// <returns>List of ParsedStatement</returns>
        public List<ParsedStatement> Parse() {

            tokens = scanner.Scan();

            while (!IsAtEnd()) {
                start = current;
                statements.Add(ParseStatement());
            }

            return statements;
        }

        private ParsedStatement ParseStatement() 
        {
            Token token = Advance();
            string delimiter = ";";
            bool setDelimiter = false;
            ParsedStatement.Type statementType = ParsedStatement.Type.Sql;

            while (!IsAtEnd() && !token.TokenType.Equals(Token.Type.Delimiter)) {

                switch(token.TokenType) {

                    case Token.Type.EndOfFile:
                        break;

                    case Token.Type.DelimiterDeclare:
                        statementType = ParsedStatement.Type.Delimiter;
                        setDelimiter = true;
                        break;

                    case Token.Type.Delimiter:
                        if (setDelimiter) {
                            // the delimiter will not be part of the statemtent's value
                            delimiter = token.Value;
                        }
                        break;

                    default:
                        break;
                }

                token = Advance();
            }

            StringBuilder builder = new StringBuilder();
            foreach (Token tokenThis in tokens.GetRange(start, (current - start))) {

                if(tokenThis.TokenType.Equals(Token.Type.EndOfLine)) {
                    builder.Append(Environment.NewLine);
                } if(tokenThis.TokenType.Equals(Token.Type.Delimiter)) {
                    // the end of the statement doesn't require a delimiter
                } else {
                    builder.Append(tokenThis.Value);
                }
            }

            return new ParsedStatement(statementType, builder.ToString(), delimiter);
        }

        private Token Advance()
        {
            current++;
            return tokens[current - 1];
        }

        private bool IsAtEnd()
        {
            bool value = false;

            if (current >= tokens.Count) {
                value = true;
            }

            return value;
        }
    }
}