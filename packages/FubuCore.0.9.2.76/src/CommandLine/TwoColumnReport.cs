using System;
using System.Linq.Expressions;
using FubuCore.Util;
using System.Linq;
using FubuCore.Reflection;

namespace FubuCore.CommandLine
{
    public class TwoColumnReport
    {
        private readonly string _title;
        private readonly Cache<string, string> _data = new Cache<string,string>();

        public TwoColumnReport(string title)
        {
            _title = title;
        }

        public ConsoleColor? SecondColumnColor { get; set; }

        public void Add(string first, string second)
        {
            _data[first] = second;
        }

        public void Add<T>(Expression<Func<T, object>> property, object target)
        {
            var accessor = property.ToAccessor();
            var rawValue = accessor.GetValue(target);
            Add(accessor.Name, rawValue == null ? " -- none --" : rawValue.ToString());
        }

        public void Write()
        {
            //this needs to take into account that the default console is only 80 char wide

            var firstLength = _data.GetAllKeys().Max(x => x.Length);
            var secondLength = _data.GetAll().Max(x => x.Length);

            var totalLength = firstLength + secondLength + 10;
            if (_title.Length > totalLength)
            {
                totalLength = _title.Length;
            }

            Console.WriteLine();

            var separator = "  ".PadRight(totalLength, '-');
            Console.WriteLine(separator);
            Console.WriteLine("    " + _title);
            Console.WriteLine(separator);

            var format = "    {0," + firstLength + "} -> ";
            if (!SecondColumnColor.HasValue)
            {
                format += "{1}";
            }

            _data.Each((left, right) =>
            {
                if (SecondColumnColor.HasValue)
                {
                    Console.Write(format, left, right);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(right);
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(format, left, right);
                }
            });

            Console.WriteLine(separator);
        }

        
    }
}