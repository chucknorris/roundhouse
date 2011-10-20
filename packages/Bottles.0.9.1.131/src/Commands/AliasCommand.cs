using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Commands
{
    public class AliasInput
    {
        [RequiredUsage("create", "remove")]
        [Description("The name of the alias")]
        public string Name { get; set; }

        [RequiredUsage("create")]
        [Description("The path to the actual folder")]
        public string Folder { get; set; }

        [ValidUsage("remove")]
        [Description("Removes the alias")]
        public bool RemoveFlag { get; set; }
    }


    [Usage("list", "List all the aliases for this solution folder")]
    [Usage("create", "Creates a new alias for a folder")]
    [Usage("remove", "Removes an alias")]
    [CommandDescription("Manage folder aliases")]
    public class AliasCommand : FubuCommand<AliasInput>
    {
        public static string AliasFolder(string folder)
        {
            //TODO: harden
            var alias = new FileSystem()
                .LoadFromFile<AliasRegistry>(AliasRegistry.ALIAS_FILE)
                .AliasFor(folder);

            return alias == null ? folder : alias.Folder;
        }


        public override bool Execute(AliasInput input)
        {
            Execute(input, new FileSystem());
            return true;
        }

        public void Execute(AliasInput input, IFileSystem system)
        {
            var registry = system.LoadFromFile<AliasRegistry>(AliasRegistry.ALIAS_FILE);
            if (input.Name.IsEmpty())
            {
                writeAliases(registry);
                return;
            }

            if (input.RemoveFlag)
            {
                registry.RemoveAlias(input.Name);
                ConsoleWriter.Write("Alias {0} removed", input.Name);
            }
            else
            {
                registry.CreateAlias(input.Name, input.Folder);
                ConsoleWriter.Write("Alias {0} created for folder {1}", input.Name, input.Folder);
            }

            persist(system, registry);
        }

        private void writeAliases(AliasRegistry registry)
        {
            if (!registry.Aliases.Any())
            {
                ConsoleWriter.Write(" No aliases are registered");
                return;
            }

            var maximumLength = registry.Aliases.Select(x => x.Name.Length).Max();
            var format = "  {0," + maximumLength + "} -> {1}";

            ConsoleWriter.Line();
            ConsoleWriter.PrintHorizontalLine();
            ConsoleWriter.Write(" Aliases:");
            ConsoleWriter.PrintHorizontalLine();

            registry.Aliases.OrderBy(x => x.Name).Each(x => { ConsoleWriter.Write(format, x.Name, x.Folder); });

            ConsoleWriter.PrintHorizontalLine();
        }

        private void persist(IFileSystem system, AliasRegistry registry)
        {
            system.WriteObjectToFile(AliasRegistry.ALIAS_FILE, registry);
        }
    }

}