using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace roundhouse.databases.ravendb.commands
{
    public class RavenGetCommand : RavenCommand
    {
        private readonly string _method;
        public RavenGetCommand(string address)
            : base(address)
        {
            _method = "GET";
        }
        

        public string Data{ get; set; }

        public override void ExecuteCommand()
        {
            var result = WebClient.DownloadString(Address);
        }

        public string Address { get; set; }
    }
}
