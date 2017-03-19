using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Healthcheck.Middleware.AspNetCore
{
    public class ProcessInfoRetriever : IProcessInfoRetriever
    {
        public ProcessInfo GetProcessInfo()
        {
            using (var currentProcess = Process.GetCurrentProcess())
            {
                var uptime = DateTime.Now - currentProcess.StartTime;
                var privateMemoryUsed = currentProcess.PrivateMemorySize64;

                return new ProcessInfo(uptime, privateMemoryUsed);
            }
        }
    }
}
