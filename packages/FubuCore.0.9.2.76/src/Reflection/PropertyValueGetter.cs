using System;
using System.Linq.Expressions;
using System.Reflection;

namespace FubuCore.Reflection
{
    public class PropertyValueGetter : IValueGetter
    {
        private readonly PropertyInfo _propertyInfo;

        public PropertyValueGetter(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
        }

        public PropertyInfo PropertyInfo { get { return _propertyInfo; } }

        public object GetValue(object target)
        {
            return _propertyInfo.GetValue(target, null);
        }

        public string Name
        {
            get { return _propertyInfo.Name; }
        }

        public Type DeclaringType
        {
            get { return _propertyInfo.DeclaringType; }
        }

        public Expression ChainExpression(Expression body)
        {
            var memberExpression = Expression.Property(body, _propertyInfo);
            if (!_propertyInfo.PropertyType.IsValueType)
            {
                return memberExpression;
            }

            return Expression.Convert(memberExpression, typeof (object));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(PropertyValueGetter)) return false;
            return Equals((PropertyValueGetter) obj);
        }

        public bool Equals(PropertyValueGetter other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other._propertyInfo.PropertyMatches(_propertyInfo);
        }

        public override int GetHashCode()
        {
            return (_propertyInfo != null ? _propertyInfo.GetHashCode() : 0);
        }
    }
}