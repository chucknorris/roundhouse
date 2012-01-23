using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FubuCore.Reflection
{
    public class PropertyChain : Accessor
    {
        private readonly IValueGetter[] _chain;
        private readonly SingleProperty _innerProperty;
        private readonly IValueGetter[] _valueGetters;


        public PropertyChain(IValueGetter[] valueGetters)
        {
            _chain = new IValueGetter[valueGetters.Length - 1];
            for (int i = 0; i < _chain.Length; i++)
            {
                _chain[i] = valueGetters[i];
            }

            var innerValueGetter = valueGetters[valueGetters.Length - 1] as PropertyValueGetter;
            if (innerValueGetter != null)
            {
                _innerProperty = new SingleProperty(innerValueGetter.PropertyInfo);
            }
            _valueGetters = valueGetters;
        }


        public void SetValue(object target, object propertyValue)
        {
            target = findInnerMostTarget(target);
            if (target == null)
            {
                return;
            }

            setValueOnInnerObject(target, propertyValue);
        }

        protected virtual void setValueOnInnerObject(object target, object propertyValue)
        {
            _innerProperty.SetValue(target, propertyValue);
        }

        public object GetValue(object target)
        {
            target = findInnerMostTarget(target);

            if (target == null)
            {
                return null;
            }

            return _innerProperty.GetValue(target);
        }

        public Type OwnerType
        {
            get 
            {
                //TODO: Does MethodValueGetter need to provide some kind of OwnerType if its the last item in the chain?
                //assuming the last item is a PropertyValueGetter for now...
                //example: Person.FamilyMembers[0].FirstName would have the _chain.Last() output a MethodValueGetter
                //in which case we would not be able to get to our OwnerType the same way as being done below

                var propertyGetter = _chain.Last() as PropertyValueGetter;
                return propertyGetter != null ? propertyGetter.PropertyInfo.PropertyType : null;
            }
        }

        public string FieldName { get { return _innerProperty.FieldName; } }

        public Type PropertyType { get { return _innerProperty.PropertyType; } }

        public PropertyInfo InnerProperty { get { return _innerProperty.InnerProperty; } }

        public Type DeclaringType { get { return _chain[0].DeclaringType; } }

        public Accessor GetChildAccessor<T>(Expression<Func<T, object>> expression)
        {
            PropertyInfo property = ReflectionHelper.GetProperty(expression);
            var list = new List<IValueGetter>(_chain)
            {
                new PropertyValueGetter(_innerProperty.InnerProperty),
                new PropertyValueGetter(property)
            };

            return new PropertyChain(list.ToArray());
        }

        public string[] PropertyNames
        {
            get { return _valueGetters.Select(x => x.Name).ToArray(); }
        }


        public Expression<Func<T, object>> ToExpression<T>()
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression body = parameter;

            _valueGetters.Each(getter =>
            {
                body = getter.ChainExpression(body);
            });

            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), typeof(object));
            return (Expression<Func<T, object>>) Expression.Lambda(delegateType, body, parameter);
        }

        public Accessor Prepend(PropertyInfo property)
        {
            var list = new List<IValueGetter>{
                new PropertyValueGetter(property)
            };
            list.AddRange(_valueGetters);

            return new PropertyChain(list.ToArray());
        }

        public IEnumerable<IValueGetter> Getters()
        {
            return _valueGetters;
        }

        public IValueGetter[] ValueGetters { get { return _valueGetters; } }


        /// <summary>
        /// Concatenated names of all the properties in the chain.
        /// Case.Site.Name == "CaseSiteName"
        /// </summary>
        public string Name
        {
            get
            {
                string returnValue = string.Empty;
                foreach (IValueGetter info in _chain)
                {
                    returnValue += info.Name;
                }

                returnValue += _innerProperty.Name;

                return returnValue;
            }
        }


        protected object findInnerMostTarget(object target)
        {
            foreach (IValueGetter info in _chain)
            {
                target = info.GetValue(target);
                if (target == null)
                {
                    return null;
                }
            }

            return target;
        }

        public override string ToString()
        {
            return _chain.First().DeclaringType.FullName + _chain.Select(x => x.Name).Join(".");
        }

        public bool Equals(PropertyChain other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (_chain.Length != other._chain.Length) return false;

            for (int i = 0; i < _chain.Length; i++)
            {
                IValueGetter info = _chain[i];
                IValueGetter otherInfo = other._chain[i];

                if (!info.Equals(otherInfo)) return false;
            }

            return _innerProperty.Equals(other._innerProperty);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (PropertyChain)) return false;
            return Equals((PropertyChain) obj);
        }

        public override int GetHashCode()
        {
            return (_chain != null ? _chain.GetHashCode() : 0);
        }
    }
}