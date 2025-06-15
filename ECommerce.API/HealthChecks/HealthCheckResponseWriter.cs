namespace ECommerce.API.HealthChecks
{
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    public static class HealthCheckResponseWriter
    {
        public static Task WriteStatusCodesJson(HttpContext ctx, HealthReport report)
        {
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsync(
                JsonSerializer.Serialize(new { status = report.Status.ToString() })
            );
        }

        public static Task WriteDetailedJson(HttpContext ctx, HealthReport report)
        {
            ctx.Response.ContentType = "application/json";
            var result = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    info = e.Value.Description
                })
            };
            return ctx.Response.WriteAsync(
                JsonSerializer.Serialize(result,
                    new JsonSerializerOptions { WriteIndented = true }
                )
            );
        }
    }
}
