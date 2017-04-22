# healthcheck-middleware-aspnetcore

ASP.NET Core middleware for rendering a JSON healthcheck page. 

```bash
$ ### This has not yet been published anywhere such as nuget
```
```cs
using Healthcheck.Middleware.AspNetCore;
```
## UseHealthcheckMiddleware([route])
Returns healthcheck middleware for a route using the given options. The middleware will return a JSON response with status 200 on success.

### Route
Passing in a route results in the middleware being automatically mapped to that route.

```cs
public void Configure(IApplicationBuilder app)
{
    app.UseHealthcheckMiddleware("/healthcheck");
}
```
> {"Status":"Success","Uptime":"00:00:44.0102266","PrivateMemoryUsed":314974208}

### Manual
In some cases, it is preferable to handle the routing manually. Map the middleware without passing in a route.

```cs
public void Configure(IApplicationBuilder app)
{
    app.Map("/healthcheck", hcApp => hcApp.UseHealthcheckMiddleware());
}
```
> {"Status":"Success","Uptime":"00:00:44.0102266","PrivateMemoryUsed":314974208}

### addChecks
A function that allows the addition of checks to the healthcheck. The function is called as `addChecks(pass, fail)`. You will call `pass()` or `fail()` depending on your desired state.

addChecks will also catch a thrown Exception but the preferred method is to call `fail()`.

```cs
public void Configure(IApplicationBuilder app)
{
    app.UseHealthcheckMiddleware("/healthcheck", new Options()
    {
        AddChecks = MyCustomHealthChecks
    });
}

private void MyCustomHealthChecks(PassHealthcheck pass, FailHealthcheck fail)
{
    try
    {
        var databaseInfo = _repository.GetDatabaseInfo();
        pass(databaseInfo);
    }
    catch (Exception)
    {
        fail(new Exception("Could not connect to the database."));
    }
}
```

#### pass
Call `pass()` when the intent is for the healthcheck to pass. Pass can be called with an `IDictionary<string, object>` that specifies additional properties to display with the health information. Calling pass will result in a status 200 and JSON message that indicates success, uptime, private memory used, and any custom properties.

If you return properties called `Status`, `Uptime` or `PrivateMemoryUsed` they will override the standard values returned.

##### Example 1
```cs
pass();
```
> {"Status":"Success","Uptime":"00:00:44.0102266","PrivateMemoryUsed":314974208}

##### Example 2
```cs
var databaseInfo = new
{
    Region = "us-west",
    Status = "ACTIVE"
};

var customPassInfo = new Dictionary<string, object>
{
    {"Database", databaseInfo}
};

pass(customPassInfo);
```
> {"Database":{"Region":"us-west","Status":"ACTIVE"}, "Status":"Success","Uptime":"00:00:44.0102266","PrivateMemoryUsed":314974208}

##### Example 3
```cs
dynamic customPassInfo = new ExpandoObject();
customPassInfo.Status = "WORKED!";

pass(customPassInfo);
```
> {"Status":"WORKED!","Uptime":"00:00:44.0102266","PrivateMemoryUsed":314974208}

#### fail
Call `fail()` when the intent is the for the healthcheck to fail. Fail accepts an Exception as an argument. Calling fail will result in a status 500 and a JSON message indicating failure with the error message.

##### Example 1
```cs
fail();
```
> {"Status":"Failure"}

##### Example 2
```cs
fail(new Exception("some error"));
```
> {"Status":"Failure","Message":"some error"}