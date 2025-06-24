using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


public class ExceptionMiddleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException vex)
            {
                // 1) FluentValidation hataları → 400 Bad Request
                _logger.LogWarning(vex, "Validation failed");
                var errors = vex.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray()
                    );

                var problem = new ValidationProblemDetails(errors)
                {
                    Type = "https://httpstatuses.com/400",
                    Title = "Geçersiz istek verisi.",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Model doğrulama hataları var.",
                    Instance = context.Request.Path
                };

                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = problem.Status.Value;
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(problem, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    })
                );
            }
            catch (KeyNotFoundException knf)
            {
                // 2) Bulunamama hatası → 404 Not Found
                _logger.LogWarning(knf, "Resource not found");
                var problem = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/404",
                    Title = knf.Message,
                    Status = StatusCodes.Status404NotFound,
                    Instance = context.Request.Path
                };
                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = problem.Status.Value;
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(problem, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    })
                );
            }
            catch (ApplicationException aex)
            {
                // 3) İş mantığı hatası → 400 Bad Request
                _logger.LogWarning(aex, "Business rule violation");
                var problem = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/400",
                    Title = aex.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Instance = context.Request.Path
                };
                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = problem.Status.Value;
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(problem, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    })
                );
            }
            catch (Exception ex)
            {
                // 4) Beklenmeyen hatalar → 500 Internal Server Error
                _logger.LogError(ex, "Unhandled exception");
                var problem = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/500",
                    Title = "Sunucu tarafında beklenmeyen bir hata oluştu.",
                    Status = StatusCodes.Status500InternalServerError,
                    Instance = context.Request.Path
                };
                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = problem.Status.Value;
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(problem, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    })
                );
            }
        }
    }
}
