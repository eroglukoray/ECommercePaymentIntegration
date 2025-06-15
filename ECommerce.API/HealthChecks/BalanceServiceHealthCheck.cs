using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace ECommerce.API.HealthChecks
{
    public class BalanceServiceHealthCheck : IHealthCheck
    {
        private readonly HttpClient _http;

        public BalanceServiceHealthCheck(IHttpClientFactory httpFactory)
        {
            _http = httpFactory.CreateClient("BalanceManagement");
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // 1 saniyelik timeout ile /api/products uç noktasını kontrol et
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(1000);

                var rsp = await _http.GetAsync("/api/products", cts.Token);
                if (rsp.IsSuccessStatusCode)
                    return HealthCheckResult.Healthy("Balance service is up");
                else
                    return HealthCheckResult.Unhealthy($"Balance returned {(int)rsp.StatusCode}");
            }
            catch (TaskCanceledException)
            {
                return HealthCheckResult.Unhealthy("Balance service timeout");
            }
            catch (HttpRequestException ex)
            {
                return HealthCheckResult.Unhealthy($"Balance service error: {ex.Message}");
            }
        }

    
    }
}
