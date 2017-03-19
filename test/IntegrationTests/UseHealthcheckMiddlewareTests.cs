using Xunit;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace Healthcheck.Middleware.AspNetCore.IntegrationTests
{
    public class UseHealthcheckMiddlewareTests
    {
        [Fact]
        public async Task ShouldReturnSuccessAndDataWhenUsedForRoute()
        {
            var webHostBuilder = new WebHostBuilder().UseStartup<UseHealthcheckMiddlewareForRouteStartup>();
            var server = new TestServer(webHostBuilder);

            using (server)
            {
                var response = await server.CreateClient().GetAsync(UseHealthcheckMiddlewareForRouteStartup.RouteUsed);
                var content = await response.Content.ReadAsStringAsync();

                Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
                Assert.Contains("Success", content);
                Assert.Contains("Uptime", content);
                Assert.Contains("PrivateMemoryUsed", content);
            }
        }

        [Fact]
        public async Task ShouldReturnSuccessAndDataWhenManuallyMapped()
        {
            var webHostBuilder = new WebHostBuilder().UseStartup<UseHealthcheckMiddlewareManualMapStartup>();
            var server = new TestServer(webHostBuilder);

            using (server)
            {
                var response = await server.CreateClient().GetAsync(UseHealthcheckMiddlewareManualMapStartup.RouteUsed);
                var content = await response.Content.ReadAsStringAsync();

                Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
                Assert.Contains("Success", content);
                Assert.Contains("Uptime", content);
                Assert.Contains("PrivateMemoryUsed", content);
            }
        }
    }

    public class UseHealthcheckMiddlewareForRouteStartup
    {
        public const string RouteUsed = "/healthcheckForRoute";
        public void Configure(IApplicationBuilder app)
        {
            app.UseHealthcheckMiddleware(RouteUsed);
        }
    }

    public class UseHealthcheckMiddlewareManualMapStartup
    {
        public const string RouteUsed = "/healthcheckManualMap";
        public void Configure(IApplicationBuilder app)
        {
            app.Map(RouteUsed, hcApp => hcApp.UseHealthcheckMiddleware());
        }
    }
}
