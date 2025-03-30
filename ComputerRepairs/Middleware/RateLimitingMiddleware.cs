using Serilog;

namespace ComputerRepairs.Middleware
{
    public class RateLimitingMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        private static int _counter = 0;
        private static DateTime _lastRequest = DateTime.Now;

        public async Task InvokeAsync(HttpContext context)
        {
            _counter++;
            if(DateTime.Now.Subtract(_lastRequest).Seconds > 5)
            {
                _counter = 1;
                _lastRequest = DateTime.Now;
                await _next(context);
            }
            else
            {
                if (_counter > 10)
                {
                    Log.Warning("User made more than 10 requests in 5 seconds");
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsync("Rate limit exceeded");
                }
                else
                {
                    _lastRequest = DateTime.Now;
                    await _next(context);
                }
            }
        }
    }
}
