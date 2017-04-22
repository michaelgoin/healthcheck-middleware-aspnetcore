using Healthcheck.Middleware.AspNetCore;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class AddChecksTests
    {
        public class WhenFunctionProvided
        {
            [Fact]
            public async Task ShouldExecuteProvidedFunction()
            {
                //Arrange
                var nextDelegate = GetPlaceholderNextDelegate();

                var mockProcessInfoRetriever = new Mock<IProcessInfoRetriever>();
                mockProcessInfoRetriever.Setup(x => x.GetProcessInfo()).Returns(new ProcessInfo(TimeSpan.Zero, 0));

                var testHttpContext = new DefaultHttpContext();


                var mockAddChecks = new Mock<AddChecks>();
                var options = new Options()
                {
                    AddChecks = mockAddChecks.Object
                };

                var healthcheckMiddleware = new HealthcheckMiddleware(nextDelegate, mockProcessInfoRetriever.Object, options);

                //Act
                await healthcheckMiddleware.Invoke(testHttpContext);

                //Assert
                mockAddChecks.Verify(addChecks => addChecks(It.IsAny<PassHealthcheck>(), It.IsAny<FailHealthcheck>()));
            }
        }

        public class WhenErrorThrown
        {
            [Fact]
            public async Task ShouldRespondWith500Status()
            {
                //Arrange
                var nextDelegate = GetPlaceholderNextDelegate();

                var mockProcessInfoRetriever = new Mock<IProcessInfoRetriever>();
                mockProcessInfoRetriever.Setup(x => x.GetProcessInfo()).Returns(new ProcessInfo(TimeSpan.FromSeconds(1), long.MaxValue));

                var testHttpContext = new DefaultHttpContext();

                var options = new Options()
                {
                    AddChecks = (pass, fail) => { throw new Exception("BOOM"); }
                };

                var healthcheckMiddleware = new HealthcheckMiddleware(nextDelegate, mockProcessInfoRetriever.Object, options);

                //Act
                await healthcheckMiddleware.Invoke(testHttpContext);

                //Assert
                Assert.Equal(500, testHttpContext.Response.StatusCode);
            }

            [Fact]
            public async Task ShouldRespondWithFailureJson()
            {
                //Arrange
                var nextDelegate = GetPlaceholderNextDelegate();

                var mockProcessInfoRetriever = new Mock<IProcessInfoRetriever>();
                mockProcessInfoRetriever.Setup(x => x.GetProcessInfo()).Returns(new ProcessInfo(TimeSpan.FromSeconds(1), long.MaxValue));

                var testHttpContext = new DefaultHttpContext();
                testHttpContext.Response.Body = new MemoryStream();

                var expected = new
                {
                    Status = HealthcheckMiddleware.FailureStatus,
                    Message =  "BOOM"
                };

                var expectedJson = JsonConvert.SerializeObject(expected);

                var options = new Options()
                {
                    AddChecks = (pass, fail) => { throw new Exception(expected.Message); }
                };

                var healthcheckMiddleware = new HealthcheckMiddleware(nextDelegate, mockProcessInfoRetriever.Object, options);

                //Act
                await healthcheckMiddleware.Invoke(testHttpContext);

                //Assert
                var response = GetResponseBodyString(testHttpContext);

                Assert.Equal("application/json", testHttpContext.Response.ContentType);
                Assert.Equal(expectedJson, response);
            }
        }

        public class WhenPassCalled
        {
            [Fact]
            public async Task ShouldRespondWith200Status()
            {
                //Arrange
                var nextDelegate = GetPlaceholderNextDelegate();

                var mockProcessInfoRetriever = new Mock<IProcessInfoRetriever>();
                mockProcessInfoRetriever.Setup(x => x.GetProcessInfo()).Returns(new ProcessInfo(TimeSpan.FromSeconds(1), long.MaxValue));

                var testHttpContext = new DefaultHttpContext();

                var options = new Options()
                {
                    AddChecks = (pass, fail) => { pass(); }
                };

                var healthcheckMiddleware = new HealthcheckMiddleware(nextDelegate, mockProcessInfoRetriever.Object, options);

                //Act
                await healthcheckMiddleware.Invoke(testHttpContext);

                //Assert
                Assert.Equal(200, testHttpContext.Response.StatusCode);
            }

            public class WithoutCustomPassInfo
            {
                [Fact]
                public async Task ShouldRespondWithStandardSuccessJson()
                {
                    //Arrange
                    var nextDelegate = GetPlaceholderNextDelegate();

                    var testHttpContext = new DefaultHttpContext();
                    testHttpContext.Response.Body = new MemoryStream();

                    var expected = new
                    {
                        Status = HealthcheckMiddleware.SuccessStatus,
                        Uptime = TimeSpan.FromMilliseconds(100),
                        PrivateMemoryUsed = 200
                    };

                    var expectedJson = JsonConvert.SerializeObject(expected);

                    var mockProcessInfoRetriever = new Mock<IProcessInfoRetriever>();
                    mockProcessInfoRetriever.Setup(x => x.GetProcessInfo()).Returns(new ProcessInfo(expected.Uptime, expected.PrivateMemoryUsed));

                    var options = new Options()
                    {
                        AddChecks = (pass, fail) => { pass(); }
                    };

                    var healthcheckMiddleware = new HealthcheckMiddleware(nextDelegate, mockProcessInfoRetriever.Object, options);

                    //Act
                    await healthcheckMiddleware.Invoke(testHttpContext);

                    //Assert
                    var response = GetResponseBodyString(testHttpContext);

                    Assert.Equal("application/json", testHttpContext.Response.ContentType);
                    Assert.Equal(expectedJson, response);
                }
            }

            public class WithCustomPassInfo
            {
                [Fact]
                public async Task ShouldRespondWithCustomPassInfoAndStandardSuccessJson()
                {
                    //Arrange
                    var nextDelegate = GetPlaceholderNextDelegate();

                    var testHttpContext = new DefaultHttpContext();
                    testHttpContext.Response.Body = new MemoryStream();

                    var databaseInfo = new
                    {
                        Region = "us-west",
                        Status = "ACTIVE"
                    };

                    var customPassInfo = new Dictionary<string, object>
                    {
                        {"Database", databaseInfo}
                    };

                    var expected = new
                    {
                        Database = databaseInfo,
                        Status = HealthcheckMiddleware.SuccessStatus,
                        Uptime = TimeSpan.FromMilliseconds(100),
                        PrivateMemoryUsed = 200
                    };

                    var expectedJson = JsonConvert.SerializeObject(expected);

                    var mockProcessInfoRetriever = new Mock<IProcessInfoRetriever>();
                    mockProcessInfoRetriever.Setup(x => x.GetProcessInfo()).Returns(new ProcessInfo(expected.Uptime, expected.PrivateMemoryUsed));

                    var options = new Options()
                    {
                        AddChecks = (pass, fail) => { pass(customPassInfo); }
                    };

                    var healthcheckMiddleware = new HealthcheckMiddleware(nextDelegate, mockProcessInfoRetriever.Object, options);

                    //Act
                    await healthcheckMiddleware.Invoke(testHttpContext);

                    //Assert
                    var response = GetResponseBodyString(testHttpContext);

                    Assert.Equal("application/json", testHttpContext.Response.ContentType);
                    Assert.Equal(expectedJson, response);
                }

                [Fact]
                public async Task ShouldRespondWithCustomPassInfoOverridesJson()
                {
                    //Arrange
                    var nextDelegate = GetPlaceholderNextDelegate();

                    var testHttpContext = new DefaultHttpContext();
                    testHttpContext.Response.Body = new MemoryStream();

                    dynamic customPassInfo = new ExpandoObject();
                    customPassInfo.Custom = "yes";
                    customPassInfo.Status = "yes";
                    customPassInfo.Uptime = TimeSpan.FromMilliseconds(1000);
                    customPassInfo.PrivateMemoryUsed = 1001;

                    var expected = new
                    {
                        customPassInfo.Custom,
                        customPassInfo.Status,
                        customPassInfo.Uptime,
                        customPassInfo.PrivateMemoryUsed
                    };

                    var expectedJson = JsonConvert.SerializeObject(expected);

                    var mockProcessInfoRetriever = new Mock<IProcessInfoRetriever>();
                    mockProcessInfoRetriever.Setup(x => x.GetProcessInfo()).Returns(new ProcessInfo(expected.Uptime, expected.PrivateMemoryUsed));

                    var options = new Options()
                    {
                        AddChecks = (pass, fail) => { pass(customPassInfo); }
                    };

                    var healthcheckMiddleware = new HealthcheckMiddleware(nextDelegate, mockProcessInfoRetriever.Object, options);

                    //Act
                    await healthcheckMiddleware.Invoke(testHttpContext);

                    //Assert
                    var response = GetResponseBodyString(testHttpContext);

                    Assert.Equal("application/json", testHttpContext.Response.ContentType);
                    Assert.Equal(expectedJson, response);
                }
            }
        }

        public class WhenFailCalled
        {
            [Fact]
            public async Task ShouldRespondWith500Status()
            {
                //Arrange
                var nextDelegate = GetPlaceholderNextDelegate();

                var mockProcessInfoRetriever = new Mock<IProcessInfoRetriever>();
                mockProcessInfoRetriever.Setup(x => x.GetProcessInfo()).Returns(new ProcessInfo(TimeSpan.FromSeconds(1), long.MaxValue));

                var testHttpContext = new DefaultHttpContext();

                var options = new Options()
                {
                    AddChecks = (pass, fail) => { fail(); }
                };

                var healthcheckMiddleware = new HealthcheckMiddleware(nextDelegate, mockProcessInfoRetriever.Object, options);

                //Act
                await healthcheckMiddleware.Invoke(testHttpContext);

                //Assert
                Assert.Equal(500, testHttpContext.Response.StatusCode);
            }

            public class WithoutError
            {
                [Fact]
                public async Task ShouldRespondWithFailureStatusJsonOnly()
                {
                    //Arrange
                    var nextDelegate = GetPlaceholderNextDelegate();

                    var testHttpContext = new DefaultHttpContext();
                    testHttpContext.Response.Body = new MemoryStream();

                    var expected = new
                    {
                        Status = HealthcheckMiddleware.FailureStatus
                    };

                    var expectedJson = JsonConvert.SerializeObject(expected);

                    var mockProcessInfoRetriever = new Mock<IProcessInfoRetriever>();

                    var options = new Options()
                    {
                        AddChecks = (pass, fail) => { fail(); }
                    };

                    var healthcheckMiddleware = new HealthcheckMiddleware(nextDelegate, mockProcessInfoRetriever.Object, options);

                    //Act
                    await healthcheckMiddleware.Invoke(testHttpContext);

                    //Assert
                    var response = GetResponseBodyString(testHttpContext);

                    Assert.Equal("application/json", testHttpContext.Response.ContentType);
                    Assert.Equal(expectedJson, response);
                }
            }

            public class WithError
            {
                [Fact]
                public async Task ShouldRespondWithFailureStatusAndErrorMessageJson()
                {
                    //Arrange
                    var nextDelegate = GetPlaceholderNextDelegate();

                    var testHttpContext = new DefaultHttpContext();
                    testHttpContext.Response.Body = new MemoryStream();

                    var exception = new Exception("BOOM");

                    var expected = new
                    {
                        Status = HealthcheckMiddleware.FailureStatus,
                        Message = exception.Message
                    };

                    var expectedJson = JsonConvert.SerializeObject(expected);

                    var mockProcessInfoRetriever = new Mock<IProcessInfoRetriever>();

                    var options = new Options()
                    {
                        AddChecks = (pass, fail) => { fail(exception); }
                    };

                    var healthcheckMiddleware = new HealthcheckMiddleware(nextDelegate, mockProcessInfoRetriever.Object, options);

                    //Act
                    await healthcheckMiddleware.Invoke(testHttpContext);

                    //Assert
                    var response = GetResponseBodyString(testHttpContext);

                    Assert.Equal("application/json", testHttpContext.Response.ContentType);
                    Assert.Equal(expectedJson, response);
                }
            }
        }

        private static RequestDelegate GetPlaceholderNextDelegate()
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
