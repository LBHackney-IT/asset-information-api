using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AssetInformationApi.V1.Middleware
{
    public class TraceLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TraceLoggingMiddleware> _logger;

        public TraceLoggingMiddleware(RequestDelegate next, ILogger<TraceLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Retrieve the trace ID from the incoming request headers
            context.Request.Headers.TryGetValue("X-Amzn-Trace-Id", out var traceId);

            _logger.LogInformation("Incoming {TraceID}", traceId);

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }
}
