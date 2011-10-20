using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FubuCore.CommandLine
{
    public class HelpInput
    {
        [IgnoreOnCommandLine]
        public IEnumerable<Type> CommandTypes { get; set; }

        [RequiredUsage("usage")]
        [Description("A command name")]
        public string Name { get; set; }

        [IgnoreOnCommandLine]
        public bool InvalidCommandName { get; set; }

        [IgnoreOnCommandLine]
        public UsageGraph Usage { get; set; }
    }
}