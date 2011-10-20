using System;
using System.Collections.Generic;
using System.Linq;

namespace FubuCore.CommandLine
{
    [Usage("list", "List all the available commands")]
    [Usage("usage", "Show all the valid usages for a command")]
    [CommandDescription("list all the available commands", Name = "help")]
    public class HelpCommand : FubuCommand<HelpInput>
    {
        // TODO -- have it write out its own usage
        // TODO -- look for command line stuff
        public override bool Execute(HelpInput input)
        {
            if (input.Usage != null)
            {
                input.Usage.WriteUsages();
                return false;
            }

            if (input.InvalidCommandName)
            {
                writeInvalidCommand(input.Name);
                listAllCommands(input);
                return false;
            }

            listAllCommands(input);
            return true;
        }

        private void listAllCommands(HelpInput input)
        {
            var report = new TwoColumnReport("Available commands:");
            input.CommandTypes.OrderBy(CommandFactory.CommandNameFor).Each(type =>
            {
                report.Add(CommandFactory.CommandNameFor(type), CommandFactory.DescriptionFor(type));
            });

            report.Write();
        }

        private void writeInvalidCommand(string commandName)
        {
            ConsoleWriter.Line();
            Console.ForegroundColor = ConsoleColor.Red;
            ConsoleWriter.Write("fubu:  '{0}' is not a command.  See available commands.", commandName);
            Console.ResetColor();
            ConsoleWriter.Line();
            ConsoleWriter.Line();
        }
    }
}