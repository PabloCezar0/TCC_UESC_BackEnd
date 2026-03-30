using Microsoft.AspNetCore.Http;
using Rota.Application.DTOs;
using System.Net;
using System.Text.Json;

namespace Rota.API.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
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

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = new ApiResponseErrorDTO
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "Ocorreu um erro inesperado.",
                Detail = _env.IsDevelopment() ? exception.ToString() : null
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = response.StatusCode;

            var result = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(result);
        }
    }
}
