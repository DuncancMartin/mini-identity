using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using IdentityCore.Models.Settings;
using Serilog.Context;

namespace IdentityApi.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        const string DOCKERBUILD = "IM-Dockerbuild";

        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IOptions<Settings> settings)
        {
            try
            {
                using (LogContext.PushProperty(DOCKERBUILD, settings.Value.DockerBuild, true))
                {
                    await _next.Invoke(context);
                }
            }
            catch (Exception ex) when (LogOnUnexpectedError(ex))
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            return exception switch
            {
                ArgumentException ex => WriteExceptionResponse(context, HttpStatusCode.BadRequest, ex.Message),
                _ => WriteExceptionResponse(context, HttpStatusCode.InternalServerError, exception.Message),
            };
        }

        private static Task WriteExceptionResponse(HttpContext context, HttpStatusCode statusCode, string error)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            var jsonResponse = JsonConvert.SerializeObject(new { error });
            return context.Response.WriteAsync(jsonResponse);
        }

        private bool LogOnUnexpectedError(Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception was thrown by the application.");
            return true;
        }
    }
}