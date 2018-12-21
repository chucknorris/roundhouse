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
        private readonly string script;

        /// <summary>
        /// Scanner to break script into tokens
        /// </summary>
        private readonly Scanner scanner;

        /// <summary>
        /// The current statement delimiter
        /// </summary>
        private string delimiter = ";";

        /// <summary>
        /// List of scanned tokens from the script
        /// </summary>
        private List<Token> tokens;

        /// <summary>
        /// List of accumulated statements
        /// </summary>
        private readonly List<ParsedStatement> statements = new List<ParsedStatement>();

        /// <summary>
        /// Flag indicating ANSI style quotes will be honored by MySQL
        /// </summary>
        private bool ansiQuotes;

        /// <summary>
        /// Start position in the token list for the current statement
        /// </summary>
        private int start;

        /// <summary>
        /// Current position in the token list
        /// </summary>
        private int current;

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

            // initialize our starting location
            start = 0;
            current = 0;

            tokens = scanner.Scan();

            while (!IsAtEnd()) {

                start = current;
                
                ParsedStatement statement = ParseStatement();
                
                if (
                    // we don't need to execute empty strings
                    statement.Value.Trim().Length > 0

                    // we don't need to execute delimiter declarations
                    && !statement.StatementType.Equals(ParsedStatement.Type.Delimiter)
                ) {

                    statements.Add(statement);
                }
            }

            return statements;
        }

        private ParsedStatement ParseStatement() 
        {
            bool setDelimiter = false;
            bool statementEnd = false;
            ParsedStatement.Type statementType = ParsedStatement.Type.Sql;

            while (!IsAtEnd() && !statementEnd) {

                Token token = Advance();
                switch(token.TokenType) {

                    case Token.Type.EndOfFile:
                        break;

                    case Token.Type.DelimiterDeclare:
                        statementType = ParsedStatement.Type.Delimiter;
                        setDelimiter = true;
                        break;

                    case Token.Type.Delimiter:
                        if (setDelimiter) {
                            delimiter = token.Value;
                            setDelimiter = false;
                        }

                        if(Peek().TokenType.Equals(Token.Type.EndOfLine)) {
                            // add the EOL to the current statement
                            Advance();
                        }

                        // this marks the end of the statement
                        statementEnd = true;
                        break;

                    default:
                        // do nothing
                        break;
                }
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

        private Token Peek() 
        {
            return tokens[current];
        }

        private bool IsAtEnd()
        {
            return current >= tokens.Count;
        }
    }
}