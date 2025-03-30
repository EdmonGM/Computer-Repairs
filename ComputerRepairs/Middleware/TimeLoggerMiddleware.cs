using Serilog;
using System.Diagnostics;

namespace ComputerRepairs.Middleware
{
    public class TimeLoggerMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            await _next(context);
            stopwatch.Stop();
            
            var request = context.Request;

            Log.Information(
                "HTTP {Method} {Path} completed in {ElapsedMilliseconds}ms with status code: {Code}",
                request.Method,
                request.Path,
                stopwatch.ElapsedMilliseconds,
                context.Response.StatusCode
                );
        }
    }
}
