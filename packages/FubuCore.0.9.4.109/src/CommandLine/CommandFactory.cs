using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FubuCore.Reflection;
using FubuCore.Util;

namespace FubuCore.CommandLine
{
    public class CommandFactory : ICommandFactory
    {
        private static readonly string[] _helpCommands = new string[]{"help", "?"}; 
        private readonly Cache<string, Type> _commandTypes = new Cache<string, Type>();
        private string _appName;

        public CommandRun BuildRun(string commandLine)
        {
            var args = StringTokenizer.Tokenize(commandLine);
            return BuildRun(args);
        }

        public CommandRun BuildRun(IEnumerable<string> args)
        {
            if (!args.Any()) return HelpRun(new Queue<string>());

            var queue = new Queue<string>(args);
            var commandName = queue.Dequeue().ToLowerInvariant();

            // TEMPORARY
            if (_helpCommands.Contains(commandName))
            {
                return HelpRun(queue);
            }

            if (_commandTypes.Has(commandName))
            {
                return buildRun(queue, commandName);
            }

            return InvalidCommandRun(commandName);
        }

        public IEnumerable<Type> AllCommandTypes()
        {
            return _commandTypes.GetAll();
        }

        public CommandRun InvalidCommandRun(string commandName)
        {
            return new CommandRun()
            {
                Command = new HelpCommand(),
                Input = new HelpInput(){
                    Name = commandName,
                    CommandTypes = _commandTypes.GetAll(),
                    InvalidCommandName = true
                }
            };
        }

        private CommandRun buildRun(Queue<string> queue, string commandName)
        {
            var command = Build(commandName);

            // this is where we'll call into UsageGraph?
            try
            {
                var usageGraph = new UsageGraph(_appName, _commandTypes[commandName]);
                var input = usageGraph.BuildInput(queue);

                return new CommandRun
                       {
                           Command = command,
                           Input = input
                       };
            }
            catch (InvalidUsageException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid usage");

                if (e.Message.IsNotEmpty())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(e.Message);
                }

                Console.ResetColor();
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error parsing input");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(e);
                Console.ResetColor();
                Console.WriteLine();
            }

            return HelpRun(commandName);
        }



        public void RegisterCommands(Assembly assembly)
        {
            assembly
                .GetExportedTypes()
                .Where(x => x.Closes(typeof(FubuCommand<>)) && x.IsConcrete())
                .Each(t => { _commandTypes[CommandNameFor(t)] = t; });
        }



        public IFubuCommand Build(string commandName)
        {
            return (IFubuCommand) Activator.CreateInstance(_commandTypes[commandName.ToLower()]);
        }



        public CommandRun HelpRun(string commandName)
        {
            return HelpRun(new Queue<string>(new []{commandName}));
        }

        public virtual CommandRun HelpRun(Queue<string> queue)
        {
            var input = (HelpInput) (new UsageGraph(_appName, typeof (HelpCommand)).BuildInput(queue));
            input.CommandTypes = _commandTypes.GetAll();


            if (input.Name.IsNotEmpty())
            {
                input.InvalidCommandName = true;
                input.Name = input.Name.ToLowerInvariant();
                _commandTypes.WithValue(input.Name, type =>
                {
                    input.InvalidCommandName = false;
                    input.Usage = new UsageGraph(_appName, type);
                });
            }

            return new CommandRun(){
                Command = new HelpCommand(),
                Input = input
            };
        }

        static readonly Regex regex = new Regex("(?<name>.+)Command",RegexOptions.Compiled);
        public static string CommandNameFor(Type type)
        {
            
            var match = regex.Match(type.Name);
            var name = type.Name;
            if(match.Success)
            {
                name = match.Groups["name"].Value;
            }
            
            type.ForAttribute<CommandDescriptionAttribute>(att => name = att.Name ?? name);

            return name.ToLower();
        }

        public static string DescriptionFor(Type type)
        {
            var description = type.FullName;
            type.ForAttribute<CommandDescriptionAttribute>(att => description = att.Description);

            return description;
        }

        public void SetAppName(string appName)
        {
            _appName = appName;
        }
    }
}