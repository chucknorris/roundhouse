using System.Collections.Generic;

namespace FubuCore.CommandLine
{
    public interface ICommandFactory
    {
        CommandRun BuildRun(string commandLine);
        CommandRun BuildRun(IEnumerable<string> args);
    }
}