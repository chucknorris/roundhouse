using System;
using System.Runtime.Serialization;

namespace Bottles.Environment
{
    [Serializable]
    public class EnvironmentRunnerException : Exception
    {
        public EnvironmentRunnerException()
        {
        }

        public EnvironmentRunnerException(string message) : base(message)
        {
        }

        protected EnvironmentRunnerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}