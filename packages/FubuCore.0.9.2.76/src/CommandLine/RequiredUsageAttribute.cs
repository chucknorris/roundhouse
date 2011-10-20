using System;

namespace FubuCore.CommandLine
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredUsageAttribute : Attribute
    {
        private readonly string[] _usages;

        public RequiredUsageAttribute(params string[] usages)
        {
            _usages = usages;
        }

        public string[] Usages
        {
            get { return _usages; }
        }
    }
}