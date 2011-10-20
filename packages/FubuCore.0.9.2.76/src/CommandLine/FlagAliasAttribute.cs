using System;

namespace FubuCore.CommandLine
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FlagAliasAttribute : Attribute
    {
        private readonly string _alias;

        public FlagAliasAttribute(string alias)
        {
            _alias = alias;
        }

        public string Alias
        {
            get { return _alias; }
        }
    }
}