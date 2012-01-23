using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using FubuCore.Conversion;

namespace FubuCore.Binding
{
    public class ValueConverterRegistry : IValueConverterRegistry
    {
        private readonly List<IConverterFamily> _families = new List<IConverterFamily>();

        public ValueConverterRegistry(IEnumerable<IConverterFamily> families, ConverterLibrary library)
        {
            if (library == null) throw new ArgumentNullException("library");

            _families.AddRange(families);

            addPolicies();

            _families.Add(new BasicConverterFamily(library));
        }

        public IEnumerable<IConverterFamily> Families
        {
            get { return _families; }
        }

        public ValueConverter FindConverter(PropertyInfo property)
        {
            var family = _families.FirstOrDefault(x => x.Matches(property));
            return family == null ? null : family.Build(this, property);
        }

        private void addPolicies()
        {
            Add<ExpandEnvironmentVariablesFamily>();
            Add<ResolveConnectionStringFamily>();

            Add<BooleanFamily>();
            Add<NumericTypeFamily>();
        }

        public void Add<T>() where T : IConverterFamily, new()
        {
            _families.Add(new T());
        }
    }
}