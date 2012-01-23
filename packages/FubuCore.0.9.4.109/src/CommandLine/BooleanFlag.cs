using System;
using System.Collections.Generic;
using System.Reflection;

namespace FubuCore.CommandLine
{
    public class BooleanFlag : TokenHandlerBase
    {
        private readonly PropertyInfo _property;

        public BooleanFlag(PropertyInfo property) : base(property)
        {
            _property = property;
        }

        public override bool Handle(object input, Queue<string> tokens)
        {
            if (tokens.NextIsFlag(_property))
            {
                tokens.Dequeue();
                _property.SetValue(input, true, null);

                return true;
            }

            return false;
        }

        public override string ToUsageDescription()
        {
            return "[{0}]".ToFormat(InputParser.ToFlagName(_property));
        }
    }
}