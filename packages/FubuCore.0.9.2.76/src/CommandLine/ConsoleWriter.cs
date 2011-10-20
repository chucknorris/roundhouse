using System;
using System.Collections.Generic;

namespace FubuCore.CommandLine
{
    public static class ConsoleWriter
    {
        public static int CONSOLE_WIDTH = 300;

        public static string HL { get; private set;}

        static ConsoleWriter()
        {
            //CONSOLE_WIDTH = Console.BufferWidth;
            HL = new string('-', CONSOLE_WIDTH);
        }

        public static void Line()
        {
            Console.WriteLine();
        }

        public static void PrintHorizontalLine()
        {
            Console.WriteLine(HL);
        }

        public static void Write(string stuff)
        {
            Write(ConsoleColor.White, stuff);
        }

        public static void WriteWithIndent(ConsoleColor color, int indent, string content)
        {
            Console.ForegroundColor = color;
            BreakIntoLines(indent, content)
                .Each(l => Console.WriteLine(l));
            Console.ResetColor();
        }
        public static void Write(ConsoleColor color, string content)
        {
            Console.ForegroundColor = color;
            BreakIntoLines(content)
                .Each(l=>Console.WriteLine(l));
            Console.ResetColor();
        }


        public static void Write(string format, params object[] args)
        {
            var input = string.Format(format, args);
            Write(input);
        }

        private static string[] BreakIntoLines(int indent, string input)
        {
            if (string.IsNullOrEmpty(input)) return new string[0];

            var lines = new List<string>();


            while (input.Length > 0)
            {
                var width = CONSOLE_WIDTH - indent;
                var chomp = input.Length > width ? width : input.Length;

                string c = new string(' ', indent) + input.Substring(0, chomp);

                lines.Add(c);
                input = input.Remove(0, chomp);
            }

            return lines.ToArray();
        }

        private static string[] BreakIntoLines(string input)
        {
            if (string.IsNullOrEmpty(input)) return new string[0];

            var lines = new List<string>();


            while(input.Length > 0)
            {
                var chomp = input.Length > CONSOLE_WIDTH ? CONSOLE_WIDTH : input.Length;
                string c = input.Substring(0, chomp);
                lines.Add(c);
                input = input.Remove(0, chomp);
            }

            return lines.ToArray();
        }

        public static void Write(ConsoleColor color, Action action)
        {
            Console.ForegroundColor = color;
            action();
            Console.ResetColor();
        }
    }
}