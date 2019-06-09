using System;

namespace roundhouse.infrastructure
{
    public class ConfigurationException: Exception
    {
        public ConfigurationException(string message) : base(message)
        {
        }
    }
}