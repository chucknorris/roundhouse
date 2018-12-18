using System;
using System.Collections.Generic;
using System.Text;
using roundhouse.databases.mysql.parser;

namespace roundhouse.databases.mysql.parser
{

    public class Parser
    {
        
        private string script;
        private Scanner scanner;
        private List<Token> tokens;
        private List<ParsedStatement> statements = new List<ParsedStatement>();
        private bool ansiQuotes;
        private int start = 0;
        private int current = 0;

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