using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using FubuCore.Conversion;

namespace FubuCore.CommandLine
{
    public class EnumerableArgument : Argument
    {
        private readonly ObjectConverter _converter;
        private readonly PropertyInfo _property;

        public EnumerableArgument(PropertyInfo property, ObjectConverter converter) : base(property, converter)
        {
            _property = property;
            _converter = converter;
        }

        public override bool Handle(object input, Queue<string> tokens)
        {
            var elementType = _property.PropertyType.GetGenericArguments().First();
            var list = typeof (List<>).CloseAndBuildAs<IList>(elementType);

            var wasHandled = false;
            while (tokens.Count > 0 && !tokens.Peek().StartsWith(InputParser.FLAG_PREFIX))
            {
                var value = _converter.FromString(tokens.Dequeue(), elementType);
                list.Add(value);

                wasHandled = true;
            }

            if (wasHandled)
            {
                _property.SetValue(input, list, null);
            }

            return wasHandled;
        }

        public override string ToUsageDescription()
        {
            return "[{0} <{1}1 {1}2 {1}3 ...>]".ToFormat(InputParser.ToFlagName(_property), _property.Name.ToLower());
        }
    }
}