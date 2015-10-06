using System;

namespace roundhouse.databases.ravendb.commands
{
    public interface IRavenCommand : IDisposable
    {
        Uri CommandAddress { get; }
        string[] CommandHeaders { get; }
        string CommandData { get;  }
        string CommandType { get; }
        int CommandTimeout { get; set; }

        object Execute();
    }
}