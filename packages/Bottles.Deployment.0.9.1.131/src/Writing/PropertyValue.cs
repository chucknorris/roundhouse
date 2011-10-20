using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using FubuCore;
using FubuCore.Reflection;

namespace Bottles.Deployment.Writing
{
    public class PropertyValue
    {
        public Accessor Accessor { get; set; }
        public object Value { get; set; }
        public string Name { get; set; }

        public string HostName { get; set; }
        
        public static PropertyValue For<T>(Expression<Func<T, object>> expression, object value)
        {
            return new PropertyValue(){
                Accessor = expression.ToAccessor(),
                Value = value
            };
        }

        public override string ToString()
        {
            var name = Name.IsNotEmpty() ? Name : toPropertyName();

            return "{0}={1}".ToFormat(name, Value.ToString());
        }

        private string toPropertyName()
        {
            var name = "{0}.{1}".ToFormat(
                Accessor.DeclaringType.Name, 
                Accessor.PropertyNames.Join("."), 
                Value == null ? string.Empty : Value.ToString());

            return HostName.IsEmpty() ? name : "{0}.{1}".ToFormat(HostName, name);
        }

        public void Write(IFlatFileWriter writer)
        {
            if (Name.IsNotEmpty())
            {
                writer.WriteProperty(Name, Value.ToString());
            }
            else
            {
                writer.WriteProperty(toPropertyName(), Value.ToString());
            }

            
        }

    }
}