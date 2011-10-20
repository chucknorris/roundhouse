using System;

namespace FubuCore.CommandLine
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandDescriptionAttribute : Attribute
    {
        private readonly string _description;

        public CommandDescriptionAttribute(string description)
        {
            _description = description;
        }

        public string Description
        {
            get { return _description; }
        }

        public string Name { get; set; }
    }
}