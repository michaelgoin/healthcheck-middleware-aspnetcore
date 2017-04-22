using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Healthcheck.Middleware.AspNetCore
{
    public class HealthcheckMiddleware
    {
        public const string SuccessStatus = "Success";
        public const string FailureStatus = "Failure";
        public const string JsonContentType = "application/json";

        private readonly RequestDelegate _next;
        private readonly IProcessInfoRetriever _processInfoRetriever;
        private readonly AddChecks _addChecks;

        public HealthcheckMiddleware(RequestDelegate next, IProcessInfoRetriever processInfoRetriever, Options options = null)
        {
            _next = next;
            _processInfoRetriever = processInfoRetriever;

            _addChecks = options?.AddChecks ?? ((pass, fail) => pass());
        }

        public async Task Invoke(HttpContext context)
        {
            var responseJson = string.Empty;

            try
            {
                _addChecks(onPass, onFail);
            }
            catch (Exception ex)
            {
                onFail(ex);
            }

            await context.Response.WriteAsync(responseJson);


            void onPass(IDictionary<string, object> customData)
            {
                var passInfo = customData ?? new Dictionary<string, object>();

                var processInfo = _processInfoRetriever.GetProcessInfo();

                passInfo.AddIfMissing("Status", SuccessStatus);
                passInfo.AddIfMissing("Uptime", processInfo.Uptime);
                passInfo.AddIfMissing("PrivateMemoryUsed", processInfo.PrivateMemoryUsed);

                responseJson = JsonConvert.SerializeObject(passInfo);
                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = JsonContentType;
                context.Response.ContentLength = responseJson.Length;
            }

            void onFail(Exception ex)
            {
                var failInfo = new Dictionary<string, object>();
                failInfo["Status"] = FailureStatus;

                if(ex != null)
                {
                    failInfo["Message"] = ex.Message;
                }

                responseJson = JsonConvert.SerializeObject(failInfo);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = JsonContentType;
                context.Response.ContentLength = responseJson.Length;
            }
        }
    }
}
