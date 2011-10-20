using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FubuCore.Util;

namespace FubuCore.Binding
{
    public class PropertyBinderCache : IPropertyBinderCache
    {
        private readonly IList<IPropertyBinder> _binders = new List<IPropertyBinder>();
        private readonly Cache<PropertyInfo, IPropertyBinder> _cache = new Cache<PropertyInfo, IPropertyBinder>();

        public PropertyBinderCache(IEnumerable<IPropertyBinder> binders, IValueConverterRegistry converters, ICollectionTypeProvider collectionTypeProvider)
        {
            _binders.AddRange(binders);
            _binders.Add(new ConversionPropertyBinder(converters));
            _binders.Add(new CollectionPropertyBinder(collectionTypeProvider));
            _binders.Add(new NestedObjectPropertyBinder());

            _cache.OnMissing = prop => _binders.FirstOrDefault(x => x.Matches(prop));
        }

        public IPropertyBinder BinderFor(PropertyInfo property)
        {
            return _cache[property];
        }
    }
}