namespace FubuCore.CommandLine
{
    public class CommandRun
    {
        public IFubuCommand Command { get; set; }
        public object Input { get; set; }

        public bool Execute()
        {
            return Command.Execute(Input);
        }
    }
}