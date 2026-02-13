using Azure;
using Azure.Core;
using DomainLayer.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.ErrorModels;
using System.Net;
using System.Reflection.Metadata;
using System.Text.Json;
namespace Cinema_Reservation.MiddleWares
{
    public class CustomExceptionHandlerMiddleware(RequestDelegate _next,ILogger<CustomExceptionHandlerMiddleware>_logger, IHostEnvironment _env)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
                if (context.Response.StatusCode == StatusCodes.Status404NotFound &&
                    !context.Response.HasStarted)
                    await HandleNotFoundEndPointExceptionAsync(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
                await HandleAllExceptionAsync(context, ex);
            }
        }

        private static async Task HandleNotFoundEndPointExceptionAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            var error = new ErrorStruct()
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = $"The endpoint '{context.Request.Path}' was not found."
            };
            var json = JsonSerializer.Serialize(error);
            await context.Response.WriteAsync(json);
        }
        private async Task HandleAllExceptionAsync(HttpContext context , Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("The response has already started, the exception middleware will not be executed.");
                return;
            }
            context.Response.ContentType = "application/json";
            var error = new ErrorStruct();
            var statusCode = ex switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                BadRequestException exception => HandleBadRequest(exception,error),
                _ => StatusCodes.Status500InternalServerError
            };
            if (statusCode == StatusCodes.Status500InternalServerError && !_env.IsDevelopment())// cant show the internal server error message to the client
                error.Message = "An unexpected internal server error occurred.";
            else error.Message = ex.Message;
            error.StatusCode = statusCode;
            context.Response.StatusCode = statusCode;
            var json = JsonSerializer.Serialize(error);
            await context.Response.WriteAsync(json);
        }

        private static int HandleBadRequest(BadRequestException exception, ErrorStruct error)
        {
            error.Errors = exception.Errors;
            return StatusCodes.Status400BadRequest;
        }
    }
}
