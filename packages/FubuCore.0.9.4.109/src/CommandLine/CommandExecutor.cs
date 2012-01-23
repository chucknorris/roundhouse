using System;

namespace FubuCore.CommandLine
{
    public class CommandExecutor
    {
        private readonly ICommandFactory _factory;

        public CommandExecutor(ICommandFactory factory)
        {
            _factory = factory;
        }

        public bool Execute(string commandLine)
        {
            var run = _factory.BuildRun(commandLine);
            return run.Execute();
        }

        public bool Execute(string[] args)
        {
            var run = _factory.BuildRun(args);
            return run.Execute();
        }
    }
}