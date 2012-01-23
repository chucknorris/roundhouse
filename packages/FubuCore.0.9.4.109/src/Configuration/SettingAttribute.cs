using System;

namespace FubuCore.Configuration
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingAttribute : Attribute
    {
        public SettingAttribute(SettingType type)
        {
            Type = type;
        }

        public string Description { get; set; }
        public SettingType Type { get; set; }
    }
}