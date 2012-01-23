using System;

namespace FubuCore.CommandLine
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ValidUsageAttribute : Attribute
    {
        private readonly string[] _usages;

        public ValidUsageAttribute(params string[] usages)
        {
            _usages = usages;
        }

        public string[] Usages
        {
            get { return _usages; }
        }
    }
}