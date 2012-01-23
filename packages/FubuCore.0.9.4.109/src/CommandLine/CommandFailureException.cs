using System;

namespace FubuCore.CommandLine
{
    [Serializable]
    public class CommandFailureException : Exception
    {
        public CommandFailureException(string message) : base(message)
        {
        }
    }
}