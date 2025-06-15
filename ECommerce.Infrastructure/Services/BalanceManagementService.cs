using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;

namespace ECommerce.Infrastructure.Services
{
    public class BalanceManagementService : IBalanceManagementService
    {
        private const string ProductsCacheKey = "CachedProducts";

        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;

        public BalanceManagementService(
            HttpClient http,
            IMemoryCache cache)
        {
            _http = http;
            _cache = cache;
        }
        public async Task<List<ProductDto>> GetProductsAsync()
        {
            try
            {
                // Uzaktan al ve cache’e kaydet
                var env = await _http.GetFromJsonAsync<ProductListEnvelope>("/api/products");
                var products = env?.Data ?? new List<ProductDto>();

                // Cache’e ekle, 5 dk TTL
                _cache.Set(
                    ProductsCacheKey,
                    products,
                    new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    }
                );

                return products;
            }
            catch
            {
                // Hata varsa cache’i dene
                if (_cache.TryGetValue(ProductsCacheKey, out List<ProductDto> cached))
                {
                    return cached;
                }
                // Cache boşsa, boş liste dön
                return new List<ProductDto>();
            }
        }

        public async Task<string> PreorderAsync(string orderId, int amount)
        {
            var req = new PreorderRequest { OrderId = orderId, Amount = amount };
            // Tam path: /api/balance/preorder
            var resp = await _http.PostAsJsonAsync("/api/balance/preorder", req);

            var env = await resp.Content
                .ReadFromJsonAsync<PreorderResponseEnvelope>();

            return (env?.Success ?? false)
                ? env.Data.PreOrder.OrderId
                : string.Empty;
        }


        public async Task<bool> CompleteOrderAsync(string reservationId)
        {
            try
            {
                // 1) doğru URL + JSON body ile POST
                var req = new CompleteRequest { OrderId = reservationId };
                var response = await _http.PostAsJsonAsync("/api/balance/complete", req);

                // 2) HTTP 2xx değilse false dön
                if (!response.IsSuccessStatusCode)
                    return false;

                // 3) JSON’u oku ve success alanına bak
                var env = await response.Content
                    .ReadFromJsonAsync<CompleteResponseEnvelope>();

                return env?.Success ?? false;
            }
            catch
            {
                // Timeout, circuit break vs. durumlarda false dön
                return false;
            }
        }
    }
}
