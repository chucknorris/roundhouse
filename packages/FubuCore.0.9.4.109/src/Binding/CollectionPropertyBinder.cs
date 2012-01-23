using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FubuCore.Util;

namespace FubuCore.Binding
{
    public class CollectionPropertyBinder : IPropertyBinder
    {
        private readonly Cache<Type,MethodInfo> _addMethods = new Cache<Type, MethodInfo>();
        private readonly ICollectionTypeProvider _collectionTypeProvider;

        public CollectionPropertyBinder(ICollectionTypeProvider collectionTypeProvider)
        {
            _collectionTypeProvider = collectionTypeProvider;
            _addMethods.OnMissing = type => type.GetMethod("Add");
        }

        public bool Matches(PropertyInfo property)
        {
            var type = property.PropertyType;
            return type.IsGenericType && type.GetInterfaces().Any(
                x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof (ICollection<>));
        }

        public void Bind(PropertyInfo property, IBindingContext context)
        {
            var type = property.PropertyType;
            var itemType = type.GetGenericArguments()[0];
            if (type.IsInterface)
            {
                type = _collectionTypeProvider.GetCollectionType(type, itemType);
            }

            var currentCollection = property.GetValue(context.Object, null);
            object collection = currentCollection ?? Activator.CreateInstance(type);
            var collectionType = collection.GetType();

            Func<object, bool> addToCollection = obj =>
                {
                    if (obj != null)
                    {
                        var addMethod = _addMethods[collectionType];
                        addMethod.Invoke(collection, new[] {obj});
                        return true;
                    }
                    return false;
                };

            var formatString = property.Name + "[{0}]";

            int index = 0;
            string prefix;
            do
            {
                prefix = formatString.ToFormat(index);
                index++;
            } while (addToCollection(context.BindObject(prefix, itemType)));

            property.SetValue(context.Object, collection, null);
        }
    }
}