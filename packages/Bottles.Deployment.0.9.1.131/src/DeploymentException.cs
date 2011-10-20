using System;
using System.Runtime.Serialization;

namespace Bottles.Deployment
{
    [Serializable]
    public class DeploymentException : Exception
    {
        public DeploymentException()
        {
        }

        public DeploymentException(string message) : base(message)
        {
        }

        public DeploymentException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DeploymentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}