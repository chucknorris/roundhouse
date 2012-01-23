using System;

namespace FubuCore
{
    public class MarkedForTerminationAttribute : Attribute
    {
        public MarkedForTerminationAttribute()
        {
        }

        public MarkedForTerminationAttribute(string description)
        {
        }
    }
}