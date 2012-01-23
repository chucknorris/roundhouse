using System;
using System.Reflection;

namespace FubuCore.Binding
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class BindingAttribute : Attribute
    {
        public abstract void Bind(PropertyInfo property, IBindingContext context);
    }
}