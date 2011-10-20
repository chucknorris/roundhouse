using System;

namespace FubuCore.CommandLine
{
    public interface IFubuCommand
    {
        bool Execute(object input);
        Type InputType { get; }
    }

    public interface IFubuCommand<T> : IFubuCommand
    {
        
    }
}