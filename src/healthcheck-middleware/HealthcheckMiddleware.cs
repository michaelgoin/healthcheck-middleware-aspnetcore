using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Healthcheck.Middleware.AspNetCore
{
    public class HealthcheckMiddleware
    {
        public const string SuccessStatus = "Success";
        public const string JsonContentType = "application/json";

        private readonly RequestDelegate _next;
        private readonly IProcessInfoRetriever _processInfoRetriever;

        public HealthcheckMiddleware(RequestDelegate next, IProcessInfoRetriever processInfoRetriever)
        {
            _next = next;
            _processInfoRetriever = processInfoRetriever;
        }

        public async Task Invoke(HttpContext context)
        {
            var processInfo = _processInfoRetriever.GetProcessInfo();

            var expected = new
            {
                Success = HealthcheckMiddleware.SuccessStatus,
                Uptime = processInfo.Uptime,
                PrivateMemoryUsed = processInfo.PrivateMemoryUsed
            };
            
            string responseJson = JsonConvert.SerializeObject(expected);

            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = JsonContentType;
            context.Response.ContentLength = responseJson.Length;

            await context.Response.WriteAsync(responseJson);
        }
    }
}
