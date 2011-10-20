using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using FubuCore.Reflection;
using System.Linq;
using FubuCore;

namespace FubuCore.CommandLine
{
    public abstract class TokenHandlerBase : ITokenHandler
    {
        private readonly PropertyInfo _property;

        protected TokenHandlerBase(PropertyInfo property)
        {
            _property = property;
        }

        public string Description
        {
            get
            {
                var name = _property.Name;
                _property.ForAttribute<DescriptionAttribute>(att => name = att.Description);

                return name;
            }
        }

        public string PropertyName { get { return _property.Name; } }

        public abstract bool Handle(object input, Queue<string> tokens);
        public abstract string ToUsageDescription();

        public virtual bool OptionalForUsage(string usage)
        {
            var returnValue =  true;
            _property.ForAttribute<ValidUsageAttribute>(att => returnValue = att.Usages.Contains(usage));

            return returnValue;
        }

        public bool Equals(TokenHandlerBase other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other._property.PropertyMatches(_property);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (TokenHandlerBase)) return false;
            return Equals((TokenHandlerBase) obj);
        }

        public override int GetHashCode()
        {
            return (_property != null ? _property.GetHashCode() : 0);
        }
    }
}