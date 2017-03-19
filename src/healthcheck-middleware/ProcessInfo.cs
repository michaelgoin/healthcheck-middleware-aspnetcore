using System;

namespace Healthcheck.Middleware.AspNetCore
{
    public class ProcessInfo
    {
        public TimeSpan Uptime { get; }

        public long PrivateMemoryUsed { get; }

        public ProcessInfo(TimeSpan uptime, long privateMemoryUsed)
        {
            Uptime = uptime;
            PrivateMemoryUsed = privateMemoryUsed;
        }
    }
}
