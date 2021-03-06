﻿using Microsoft.AspNetCore.Builder;

namespace Healthcheck.Middleware.AspNetCore
{
    public static class HealthcheckExtensions
    {
        public static IApplicationBuilder UseHealthcheckMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<HealthcheckMiddleware>(new ProcessInfoRetriever());
        }

        public static IApplicationBuilder UseHealthcheckMiddleware(this IApplicationBuilder app, Options options)
        {
            return app.UseMiddleware<HealthcheckMiddleware>(new ProcessInfoRetriever(), options);
        }

        public static IApplicationBuilder UseHealthcheckMiddleware(this IApplicationBuilder app, string route)
        {
            return app.Map(route, hcApp => hcApp.UseHealthcheckMiddleware());
        }

        public static IApplicationBuilder UseHealthcheckMiddleware(this IApplicationBuilder app, string route, Options options)
        {
            return app.Map(route, hcApp => hcApp.UseHealthcheckMiddleware(options));
        }
    }
}
