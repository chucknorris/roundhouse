using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FubuCore.Conversion;
using FubuCore.Reflection;

namespace FubuCore.CommandLine
{
    public class Argument : TokenHandlerBase
    {
        private readonly ObjectConverter _converter;
        private readonly PropertyInfo _property;
        private bool _isLatched;

        public Argument(PropertyInfo property, ObjectConverter converter) : base(property)
        {
            _property = property;
            _converter = converter;
        }

        public override bool Handle(object input, Queue<string> tokens)
        {
            if (_isLatched) return false;

            if (tokens.Peek().StartsWith(InputParser.FLAG_PREFIX)) return false;

            var value = _converter.FromString(tokens.Dequeue(), _property.PropertyType);
            _property.SetValue(input, value, null);

            _isLatched = true;

            return true;
        }

        public override string ToUsageDescription()
        {
            if (_property.PropertyType.IsEnum)
            {
                return Enum.GetNames(_property.PropertyType).Join("|");
            }

            return "<{0}>".ToFormat(_property.Name.ToLower());
        }

        public bool RequiredForUsage(string usage)
        {
            var returnValue = false;
            _property.ForAttribute<RequiredUsageAttribute>(att => { returnValue = att.Usages.Contains(usage); });

            return returnValue;
        }

        public override bool OptionalForUsage(string usage)
        {
            return false;
        }
    }
}