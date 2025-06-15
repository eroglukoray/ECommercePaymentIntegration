using ECommerce.Application.DTOs;
using FluentAssertions;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace ECommerce.Test.Integration
{
    public class OrdersApiTests : IClassFixture<CustomWebAppFactory>
    {
        private readonly HttpClient _client;

        public OrdersApiTests(CustomWebAppFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetProducts_ReturnsMockedProduct()
        {
            // Act
            var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products");

            // Assert
            products.Should().ContainSingle()
                .Which.Name.Should().Be("TestProduct");
        }

        [Fact]
        public async Task CreateAndCompleteOrder_FullFlow_Works()
        {
            // Arrange: ürün listesi
            var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products");
            products.Should().ContainSingle();
            var productId = products[0].Id;

            // Act: sipariş oluştur
            var createResp = await _client.PostAsJsonAsync(
                "/api/orders/create",
                new { ProductIds = new List<string> { productId } });

            // Assert create
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var orderResult = await createResp.Content.ReadFromJsonAsync<OrderResultDto>();
            orderResult.Should().NotBeNull();
            orderResult!.OrderId.Should().NotBeNullOrEmpty();
            orderResult.Status.Should().Be("Reserved");

            // Act: siparişi tamamla
            var completeResp = await _client.PostAsync(
                $"/api/orders/{orderResult.OrderId}/complete", null);

            // Assert complete
            completeResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }

}
