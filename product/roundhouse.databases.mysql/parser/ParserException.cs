namespace roundhouse.databases.mysql.parser
{
    /// <summary>
    /// Exception for all parsing and scanning related exceptions.
    /// </summary>
    public class ParserException : System.Exception
    {
        /// <summary>
        /// Creates a new exception and sets its fields
        /// </summary>
        /// <param name="message">the exception's message</param>
        public ParserException(string message) : base(message)
        {

        }
    }
}