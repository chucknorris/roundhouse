using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Bottles.Diagnostics;
using FubuCore;
using System.Text;
using System.Threading;

namespace Bottles.Deployment
{
    public class ProcessReturn
    {
        public string OutputText { get; set; }
        public int ExitCode { get; set; }

        public void AssertOptionalSuccess()
        {
            LogWriter.Current.Trace(OutputText);
        }

        public void AssertMandatorySuccess()
        {
            AssertMandatorySuccess(code => code > 0);
        }

        public void AssertMandatorySuccess(Func<int, bool> exitCodeFails)
        {
            if (exitCodeFails(ExitCode))
            {
                LogWriter.Current.Fail(OutputText);
            }
            else
            {
                LogWriter.Current.Trace(OutputText);
            }
        }
    }


    public class ProcessRunner : IProcessRunner
    {
        public ProcessReturn Run(ProcessStartInfo info, TimeSpan waitDuration)
        {
            //use the operating system shell to start the process
            //this allows credentials to flow through.
            //info.UseShellExecute = true; 
            info.UseShellExecute = false;
            info.Verb = "runas";

            //don't open a new terminal window
            info.CreateNoWindow = true;

            info.RedirectStandardError = info.RedirectStandardOutput = true;

            LogWriter.Current.Trace("Running process at {0} {1}\nIn working directory {2}", info.FileName, info.Arguments, info.WorkingDirectory);
            
            if (!Path.IsPathRooted(info.FileName))
            {
                info.FileName = info.WorkingDirectory.AppendPath(info.FileName);
            }

            ProcessReturn returnValue = null;
			var output = new StringBuilder();
            int pid = 0;
            using (var proc = Process.Start(info))
            {
                pid = proc.Id;				
				proc.OutputDataReceived += (sender, outputLine) => 
				{ 
					output.Append(outputLine.Data); 
				};
				
				proc.BeginOutputReadLine();
                proc.WaitForExit((int)waitDuration.TotalMilliseconds);
				
				killProcessIfItStillExists(pid);
				
                returnValue = new ProcessReturn(){              
					ExitCode = proc.ExitCode,
                    OutputText = output.ToString()
                };                
            }

            return returnValue;
        }

        private void killProcessIfItStillExists(int pid)
        {
            if (Process.GetProcesses()
                .Where(p => p.Id == pid)
                .Any())
            {
                try
                {
                    var p = Process.GetProcessById(pid);
                    if(!p.HasExited)
                    {
                        p.Kill();
						Thread.Sleep(100);
                    }
                }
                catch (ArgumentException)
                {
                    //ignore
                }
            }
        }

        public ProcessReturn Run(ProcessStartInfo info)
        {
            return Run(info, new TimeSpan(0,0,0,10));
        }
    }
}