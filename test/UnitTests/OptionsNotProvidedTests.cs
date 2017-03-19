using Xunit;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Moq;
using System;

namespace Healthcheck.Middleware.AspNetCore.UnitTests
{
    public class OptionsNotProvidedTests
    {
        [Fact]
        public async Task ShouldRespondWith200Status()
        {
            //Arrange
            var nextDelegate = GetPlaceholderNextDelegate();
            
            Mock<IProcessInfoRetriever> mockProcessInfoRetriever = new Mock<IProcessInfoRetriever>();
            mockProcessInfoRetriever.Setup(x => x.GetProcessInfo()).Returns(new ProcessInfo(TimeSpan.Zero, 0));

            var testHttpContext = new DefaultHttpContext();
            var healthcheckMiddleware = new HealthcheckMiddleware(nextDelegate, mockProcessInfoRetriever.Object);

            //Act
            await healthcheckMiddleware.Invoke(testHttpContext);

            //Assert
            Assert.Equal(200, testHttpContext.Response.StatusCode);
        }
        
        [Fact]
        public async Task ShouldRespondWithJsonContainingSuccessfulStatusUptimeAndMemoryUsage()
        {
            //Arrange
            var nextDelegate = GetPlaceholderNextDelegate();

            var testHttpContext = new DefaultHttpContext();
            testHttpContext.Response.Body = new MemoryStream();

            var expected = new
            {
                Success = HealthcheckMiddleware.SuccessStatus,
                Uptime = TimeSpan.FromMilliseconds(100),
                PrivateMemoryUsed = 200
            };

            var expectedJson = JsonConvert.SerializeObject(expected);

            Mock<IProcessInfoRetriever> mockProcessInfoRetriever = new Mock<IProcessInfoRetriever>();
            mockProcessInfoRetriever.Setup(x => x.GetProcessInfo()).Returns(new ProcessInfo(expected.Uptime, expected.PrivateMemoryUsed));

            var healthcheckMiddleware = new HealthcheckMiddleware(nextDelegate, mockProcessInfoRetriever.Object);

            //Act
            await healthcheckMiddleware.Invoke(testHttpContext);

            //Assert
            var response = GetResponseBodyString(testHttpContext);

            Assert.Equal("application/json", testHttpContext.Response.ContentType);
            Assert.Equal(expectedJson, response);
        }

        private RequestDelegate GetPlaceholderNextDelegate()
        {
            return (innerHttpContext) => Task.FromResult(0);
        }

        private static string GetResponseBodyString(HttpContext context)
        {
            var result = Encoding.UTF8.GetString(GetWrittenBytes(context));
            return result;
        }

        private static byte[] GetWrittenBytes(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            return Assert.IsType<MemoryStream>(context.Response.Body).ToArray();
        }
    }
}
