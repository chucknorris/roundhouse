using System;
using System.Collections.Generic;
using System.Text;
using roundhouse.databases.mysql.parser;

namespace roundhouse.databases.mysql 
{

    class Parser
    {
        
        private string script;
        private Scanner scanner;

        public Parser(string script)
        {
            this.script = script;
            this.scanner = new Scanner(script);
        }

        public void Test()
        {
            List<Token> tokens = scanner.Scan();
        }
    }
}