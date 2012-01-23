using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace FubuCore.Binding
{
    [Serializable]
    public class ConvertProblem
    {
        public object Item { get; set; }
        public object Value { get; set; }
        public string ExceptionText { get; set; }
        public IEnumerable<PropertyInfo> Properties { get; set; }

        public override string ToString()
        {
            return
                @"Item type:       {0}
Property:        {1}
Property Type:   {2}
Attempted Value: {3}
Exception:
{4} 
"
                    .ToFormat(
                    ((Item != null) ? Item.GetType().FullName : "(null)"),
                    PropertyName(),
                    Properties.Last().PropertyType,
                    Value,
                    ExceptionText);
        }

        public string PropertyName()
        {
            return Properties.Select(x => x.Name).Join(".");
        }
    }
}