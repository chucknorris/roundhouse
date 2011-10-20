using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FubuCore.Reflection;
using FubuCore;

namespace Bottles.Deployment.Writing
{
    public class HostDefinition
    {
        private readonly string _name;
        private readonly IList<IDirective> _directives = new List<IDirective>();
        private readonly IList<BottleReference> _references = new List<BottleReference>();
        private readonly IList<PropertyValue> _values = new List<PropertyValue>();

        public HostDefinition(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public void AddReference(BottleReference reference)
        {
            _references.Add(reference);
        }

        public void AddDirectives(IEnumerable<IDirective> directives)
        {
            _directives.AddRange(directives);
        }

        public void AddDirective(IDirective directive)
        {
            _directives.Add(directive);
        }

        public void AddProperty<T>(Expression<Func<T, object>> expression, object value)
        {
            _values.Add(new PropertyValue(){
                Accessor = expression.ToAccessor(),
                Value = value
            });
        }

        public void AddProperty(string key, string value)
        {
            _values.Add(new PropertyValue(){
                Name = key,
                Value = value
            });
        }

        public IEnumerable<IDirective> Directives
        {
            get { return _directives; }
        }

        public IEnumerable<BottleReference> References
        {
            get { return _references; }
        }

        public IEnumerable<PropertyValue> Values
        {
            get { return _values; }
        }

        public string FileName
        {
            get { return "{0}.host".ToFormat(Name); }
        }
    }
}