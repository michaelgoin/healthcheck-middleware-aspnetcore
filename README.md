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
> {Status: 'Success', Uptime: 3, PrivateMemoryUsed: 32587776}

### Manual
In some cases, it is preferable to handle the routing manually. Map the middleware without passing in a route.

```cs
public void Configure(IApplicationBuilder app)
{
    app.Map("/healthcheck", hcApp => hcApp.UseHealthcheckMiddleware());
}
```
> {Status: 'Success', Uptime: 3, PrivateMemoryUsed: 32587776}