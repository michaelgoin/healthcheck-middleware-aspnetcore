namespace Healthcheck.Middleware.AspNetCore
{
    public interface IProcessInfoRetriever
    {
        ProcessInfo GetProcessInfo();
    }
}