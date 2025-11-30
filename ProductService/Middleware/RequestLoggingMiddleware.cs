using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ProductService.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var request = context.Request;

        string body = "";
        if (request.Method != HttpMethod.Get.Method && request.ContentLength > 0)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        await _next(context);
        sw.Stop();

        _logger.LogInformation(
            "HTTP {Method} {Path} | Status: {StatusCode} | {Elapsed} ms | IP: {IP} | Body: {Body}",
            request.Method,
            request.Path,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds,
            context.Connection.RemoteIpAddress?.ToString(),
            body
        );
    }
}
