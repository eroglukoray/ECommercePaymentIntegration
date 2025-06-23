using FluentValidation;
using System.Text.Json;


public class ExceptionMiddleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next,
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
            catch (ValidationException vex) // FluentValidation
            {
                _logger.LogWarning(vex, "Validation error");
                await WriteProblemDetails(
                    context,
                    StatusCodes.Status400BadRequest,
                    "Geçersiz istek verisi.",
                    new { errors = vex.Errors }
                );
            }
            catch (KeyNotFoundException knf)
            {
                _logger.LogWarning(knf, "Not found error");
                await WriteProblemDetails(
                    context,
                    StatusCodes.Status404NotFound,
                    knf.Message
                );
            }
            catch (ApplicationException aex)
            {
                _logger.LogWarning(aex, "Application error");
                await WriteProblemDetails(
                    context,
                    StatusCodes.Status400BadRequest,
                    aex.Message
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected server error");
                await WriteProblemDetails(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "Sunucu tarafında beklenmeyen bir hata oluştu."
                );
            }
        }

        private static Task WriteProblemDetails(
            HttpContext ctx,
            int statusCode,
            string title,
            object? additional = null)
        {
            ctx.Response.ContentType = "application/problem+json";
            ctx.Response.StatusCode = statusCode;

            var problem = new
            {
                type = $"https://httpstatuses.com/{statusCode}",
                title,
                status = statusCode,
                traceId = ctx.TraceIdentifier,
                detail = additional
            };

            var json = JsonSerializer.Serialize(problem,
                new JsonSerializerOptions { WriteIndented = true });

            return ctx.Response.WriteAsync(json);
        }
    }
}
