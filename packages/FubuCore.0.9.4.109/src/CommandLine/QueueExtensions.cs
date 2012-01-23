using System.Collections.Generic;
using System.Reflection;

namespace FubuCore.CommandLine
{
    public static class QueueExtensions
    {
        public static bool NextIsFlag(this Queue<string> queue, PropertyInfo property)
        {
            return queue.Peek().ToLower() == InputParser.ToFlagName(property);
        }
    }
}