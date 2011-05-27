// ==============================================================================
// 
// Fervent Coder Copyright © 2011 - Released under the Apache 2.0 License
// 
// Copyright 2007-2008 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
//
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
// ==============================================================================
namespace $rootnamespace$
{
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    public class CommandRunner
    {
        #region Methods

        public static int run(string process, string arguments, bool wait_for_exit)
        {
            var exit_code = -1;
            var psi = new ProcessStartInfo(Path.GetFullPath(process), arguments)
            {
                WorkingDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                UseShellExecute = false,
                RedirectStandardOutput = false,
                CreateNoWindow = false
            };

            using (var p = new Process())
            {
                p.StartInfo = psi;
                p.Start();
                if (wait_for_exit)
                {
                    p.WaitForExit();
                }
                exit_code = p.ExitCode;
            }

            return exit_code;
        }

        #endregion
    }
}