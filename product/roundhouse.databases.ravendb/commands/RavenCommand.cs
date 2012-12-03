using System;
using System.Net;

namespace roundhouse.databases.ravendb.commands
{
    public interface IRavenCommand : IDisposable
    {
        string ExecuteCommand();
        string Address { get; set; }
    }

    public abstract class RavenCommand : IRavenCommand
    {
        protected readonly WebClient WebClient;

        protected RavenCommand(string address)
        {
            Address = address;
            WebClient = new WebClient();
        }

        public void Dispose()
        {
            WebClient.Dispose();
        }

        public abstract string ExecuteCommand();
        public string Address { get; set; }
    }

    
}