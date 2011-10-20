using System;
using System.Collections.Generic;
using System.Reflection;

namespace FubuCore.CommandLine
{
    public class Flag : TokenHandlerBase
    {
        private readonly PropertyInfo _property;
        private readonly ObjectConverter _converter;

        public Flag(PropertyInfo property, ObjectConverter converter) : base(property)
        {
            _property = property;
            _converter = converter;
        }

        public override bool Handle(object input, Queue<string> tokens)
        {
            if (tokens.NextIsFlag(_property))
            {
                tokens.Dequeue();
                var rawValue = tokens.Dequeue();
                var value = _converter.FromString(rawValue, _property.PropertyType);

                _property.SetValue(input, value, null);

                return true;
            }


            return false;
        }

        public override string ToUsageDescription()
        {
            var flagName = InputParser.ToFlagName(_property);

            if (_property.PropertyType.IsEnum)
            {
                var enumValues = Enum.GetNames(_property.PropertyType).Join("|");
                return "[{0} {1}]".ToFormat(flagName, enumValues);
            }

            
            return "[{0} <{1}>]".ToFormat(flagName, _property.Name.ToLower().TrimEnd('f', 'l','a','g'));
        }
    }
}