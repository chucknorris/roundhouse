using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using roundhouse.sqlsplitters;

namespace roundhouse.databases.sqlserver
{
    /// <summary>
    /// Class which splits SQL Server statements based on batch identifiers.
    /// </summary>
    public class SqlServerStatementSplitter : StatementSplitter
    {

        /// <summary>
        /// Splits the sql using the BreakIntoBatches method, removing empty and whitespace only batches.
        /// </summary>
        /// <param name="sql_batches"></param>
        /// <returns></returns>
        public IEnumerable<string> split(string sql_batches)
        {
            // regular expression to match something other than
            // whitespace.
            var nonWhitespace = new Regex(@"\S");
            return BreakIntoBatches(sql_batches).Where(batch => batch.Any() && nonWhitespace.IsMatch(batch));
        }

        private class Token
        {
            public Token(TokenType type, int index, int length)
            {
                Type = type;
                Index = index;
                Length = length;
            }

            public TokenType Type { get; private set; }
            public int Index { get; private set; }
            public int Length { get; private set; }
        }

        private enum TokenType
        {
            BATCH_SEPERATOR,
            STRING_DELIMITER,
            MULTI_LINE_COMMENT_START,
            MULTI_LINE_COMMENT_END,
            SINGLE_LINE_COMMENT_START,
            NEW_LINE
        }

        /// <summary>
        /// Break the sql batch into a sequence of tokens
        /// </summary>
        /// <param name="sql_batch">The sql that we are tokenizing (including batch seperators)</param>
        /// <returns>The sequence of tokens that exists in the sql batch.</returns>
        private IEnumerator<Token> Tokenizer(string sql_batch)
        {
            // Single unified token pattern so we only do one pass over the whole thing.
            var tokenPattern = new Regex(@"(\b(?<BATCH_SEPERATOR>GO)\b)|
                                    (?<STRING_DELIMITER>')|
                                    (?<MULTI_LINE_COMMENT_START>/\*)|
                                    (?<MULTI_LINE_COMMENT_END>\*/)|
                                    (?<SINGLE_LINE_COMMENT_START>--)|
                                    (?<NEW_LINE>$)",
                                  RegexOptions.Multiline | 
                                  RegexOptions.IgnorePatternWhitespace |
                                  RegexOptions.ExplicitCapture |
                                  RegexOptions.IgnoreCase);
            var match = tokenPattern.Match(sql_batch);
            while(match.Success)
            {
                // unfortunately need to query for each individual 
                // capturing group rather than working backwards to find
                // the token type. (Also GetGroupNames() returns "0" as well as the names, so need
                // to use the enums and ToString().
                foreach(var tokenType in (TokenType[])Enum.GetValues(typeof(TokenType)))
                {
                    var group = match.Groups[tokenType.ToString()];
                    if(group.Success)
                    {
                        yield return new Token(
                            type: tokenType,
                            index: group.Index,
                            length: group.Length
                        );
                        break;
                    }
                }

                match = match.NextMatch();
            }
        }

        /// <summary>
        /// Break the sql into a sequence of batches.
        /// </summary>
        /// <param name="sql_batch">The complete sql including batch seperators</param>
        /// <returns>The sql batches</returns>
        private IEnumerable<string> BreakIntoBatches(string sql_batch)
        {
            // We use a tokenizer to convert the batch into a sequence
            // of tokens so we know when to split it, taking into account
            // string literals and comments.
            using (var tokens = Tokenizer(sql_batch))
            {
                int currentCutIndex = 0;

                Token token = null;
                // Helper function to set the current token
                // and to let us know if there are anymore tokens.
                Func<bool> getNextToken = () =>
                                              {
                                                  if (tokens.MoveNext())
                                                  {
                                                      token = tokens.Current;
                                                      return true;
                                                  }
                                                  return false;
                                              };

                while (getNextToken())
                {
                    switch (token.Type)
                    {
                        case TokenType.BATCH_SEPERATOR:
                            yield return sql_batch.Substring(currentCutIndex, token.Index - currentCutIndex);
                            currentCutIndex = token.Index + token.Length;
                            break;
                        case TokenType.STRING_DELIMITER:
                            // skip over everything until we get out of the string.
                            while(getNextToken())
                            {
                                if(token.Type == TokenType.STRING_DELIMITER)
                                {
                                    break;
                                }
                            }
                            break;
                        case TokenType.MULTI_LINE_COMMENT_START:
                            // run until we get out of the comment.
                            int commentDepth = 1;
                            while(commentDepth > 0 && getNextToken())
                            {
                                if(token.Type == TokenType.MULTI_LINE_COMMENT_START)
                                {
                                    commentDepth++;
                                }
                                if(token.Type == TokenType.MULTI_LINE_COMMENT_END)
                                {
                                    commentDepth--;
                                }
                            }
                            break;
                        case TokenType.SINGLE_LINE_COMMENT_START:
                            // consume until we get out of the comment
                            while(getNextToken())
                            {
                                if(token.Type == TokenType.NEW_LINE)
                                {
                                    // done here.
                                    break;
                                }
                            }
                            break;
                        case TokenType.NEW_LINE:
                            // all good, just skip these. 
                            break;
                        // Shouldn't get here:
                        case TokenType.MULTI_LINE_COMMENT_END:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                // Remember to return the remainder. (There isn't an end of document token).
                yield return sql_batch.Substring(currentCutIndex);
            }
        }
    }
}
