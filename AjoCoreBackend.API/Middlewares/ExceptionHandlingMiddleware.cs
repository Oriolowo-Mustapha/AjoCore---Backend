using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using AjoCoreBackend.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace AjoCoreBackend.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            int statusCode;
            object response;

            switch (exception)
            {
                case ValidationException validationException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    var errors = validationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                    response = new { errors };
                    break;

                case NotFoundException notFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    response = new { error = notFoundException.Message };
                    break;

                case DuplicateWebhookException duplicateException:
                    statusCode = (int)HttpStatusCode.Conflict;
                    response = new { error = duplicateException.Message };
                    break;

                case DomainException domainException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    response = new { error = domainException.Message };
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    response = new { error = "An unexpected error occurred." };
                    break;
            }

            context.Response.StatusCode = statusCode;
            var result = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            
            return context.Response.WriteAsync(result);
        }
    }
}
