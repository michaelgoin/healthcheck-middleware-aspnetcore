using System;
using System.Collections.Generic;
using System.Text;

namespace Healthcheck.Middleware.AspNetCore
{
    public delegate void PassHealthcheck(IDictionary<string, object> customData = null);
    public delegate void FailHealthcheck(Exception ex = null);
    public delegate void AddChecks(PassHealthcheck pass, FailHealthcheck fail);

    public class Options
    {
        public AddChecks AddChecks { get; set; }
    }
}
