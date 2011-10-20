using System;
using System.Runtime.Serialization;
using FubuCore;

namespace Bottles.Deployment.Configuration
{
    [Serializable]
    public class EnvironmentSettingsException : Exception
    {
        private static readonly string _validUsage =
            "Environment settings must be in the form '[Prop]=[Value]' or '[Host].[Directive].[Property]=[Value], but was\n{0}";


        public EnvironmentSettingsException(string actual) : base(_validUsage.ToFormat(actual))
        {
        }

        protected EnvironmentSettingsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}