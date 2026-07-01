using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using AjoCoreBackend.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AjoCoreBackend.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger logger)
        {
            var requestId = context.TraceIdentifier;
            logger.LogError(exception, "An unhandled exception occurred during request {RequestId}. Path: {Path}", requestId, context.Request.Path);
            
            context.Response.ContentType = "application/json";
            
            int statusCode;
            string code;
            string message;
            object details = null;

            switch (exception)
            {
                case ValidationException validationException:
                    statusCode = (int)HttpStatusCode.UnprocessableEntity; // 422 for validation
                    code = "VALIDATION_ERROR";
                    message = "One or more validation errors occurred.";
                    details = validationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                    break;

                case NotFoundException notFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    code = "NOT_FOUND";
                    message = notFoundException.Message;
                    break;

                case ForbiddenAccessException forbiddenException:
                    statusCode = (int)HttpStatusCode.Forbidden;
                    code = "FORBIDDEN";
                    message = forbiddenException.Message;
                    break;

                case InvalidCredentialsException credentialsException:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    code = "UNAUTHORIZED";
                    message = credentialsException.Message;
                    break;

                case DuplicateEmailException duplicateEmailException:
                    statusCode = (int)HttpStatusCode.Conflict;
                    code = "CONFLICT";
                    message = duplicateEmailException.Message;
                    break;

                case DuplicateWebhookException duplicateException:
                    statusCode = (int)HttpStatusCode.Conflict;
                    code = "CONFLICT";
                    message = duplicateException.Message;
                    break;

                case DomainException domainException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    code = "BAD_REQUEST";
                    message = domainException.Message;
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    code = "INTERNAL_SERVER_ERROR";
                    message = "An unexpected error occurred.";
                    break;
            }

            context.Response.StatusCode = statusCode;

            var response = new 
            {
                error = new 
                {
                    code,
                    message,
                    details,
                    requestId
                }
            };

            var result = JsonSerializer.Serialize(response, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            
            return context.Response.WriteAsync(result);
        }
    }
}
