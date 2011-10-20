using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;
using FubuCore.Reflection;

namespace Bottles.Deployment.Writing
{
    public class DirectiveWriter
    {
        private readonly IFlatFileWriter _writer;
        private readonly ITypeDescriptorCache _types;
        private readonly Stack<string> _names = new Stack<string>();
        private string _prefix;

        public DirectiveWriter(IFlatFileWriter writer, ITypeDescriptorCache types)
        {
            _writer = writer;
            _types = types;
        }

        private void setPrefix(Action<Stack<string>> configure)
        {
            configure(_names);
            _prefix =  _names.Reverse().Join(".") + ".";
        }

        public void Write(object directive)
        {
            var type = directive.GetType();
            setPrefix(x => x.Push(type.Name));

            write(directive, type);

            setPrefix(x => x.Pop());
        }

        private void write(object directive, Type type)
        {
            _types.ForEachProperty(type, prop =>
            {
                var child = prop.GetValue(directive, null);

                if (prop.PropertyType.IsSimple())
                {
                    var stringValue = child == null ? string.Empty : child.ToString();
                    var name = "{0}{1}".ToFormat(_prefix, prop.Name);

                    _writer.WriteProperty(name, stringValue);
                }
                else
                {
                    if (child != null)
                    {
                        setPrefix(x => x.Push(prop.Name));
                        write(child, child.GetType());
                        setPrefix(x => x.Pop());
                    }
                }
            });
        }
    }
}