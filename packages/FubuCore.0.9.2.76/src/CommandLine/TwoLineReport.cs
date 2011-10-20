using System;
using System.Linq.Expressions;
using FubuCore.Reflection;
using FubuCore.Util;

namespace FubuCore.CommandLine
{
    public class TwoLineReport
    {
        private readonly string _title;
        private readonly Cache<string, string> _data = new Cache<string, string>();

        public TwoLineReport(string title)
        {
            _title = title;
            SecondLineColor = ConsoleColor.Cyan;
        }

        public ConsoleColor SecondLineColor { get; set; }

        public void Add(string first, string second)
        {
            _data[first] = second;
        }

        public void Add<T>(Expression<Func<T, object>> property, object target)
        {
            var accessor = property.ToAccessor();
            var rawValue = accessor.GetValue(target);
            Add(accessor.Name, rawValue == null ? "-- none --" : rawValue.ToString());
        }

        public void Write()
        {
            ConsoleWriter.Line();


            ConsoleWriter.PrintHorizontalLine();
            Console.WriteLine(_title);
            ConsoleWriter.PrintHorizontalLine();

            _data.Each((left, right) =>
            {
                ConsoleWriter.Write(left);
                ConsoleWriter.WriteWithIndent(SecondLineColor,4, right);
            });

            ConsoleWriter.PrintHorizontalLine();
        }
    }
}