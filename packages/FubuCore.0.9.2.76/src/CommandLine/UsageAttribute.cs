using System;

namespace FubuCore.CommandLine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class UsageAttribute : Attribute
    {
        private readonly string _description;
        private readonly string _name;

        public UsageAttribute(string name, string description)
        {
            _name = name;
            _description = description;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Description
        {
            get { return _description; }
        }
    }
}