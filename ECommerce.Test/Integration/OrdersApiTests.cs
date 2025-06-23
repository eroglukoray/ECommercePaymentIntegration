using ECommerce.Application.DTOs;
using FluentAssertions;
using System;
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
            // 1) Ürünleri al, 200 OK olmalı
            var products = await _client.GetFromJsonAsync<ProductDto[]>("/api/products");
            Assert.NotNull(products);
            Assert.NotEmpty(products);

            // 2) Sipariş oluşturma isteği
            var createReq = new CreateOrderRequest
            {
                ProductIds = new List<string> { products[0].Id }
            };
            var idemKeyCreate = Guid.NewGuid().ToString();

            var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/orders/create")
            {
                Content = JsonContent.Create(createReq)
            };
            createMsg.Headers.Add("Idempotency-Key", idemKeyCreate);

            var createResp = await _client.SendAsync(createMsg);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

            var created = await createResp.Content.ReadFromJsonAsync<OrderResultDto>();
            Assert.NotNull(created);
            Assert.False(string.IsNullOrWhiteSpace(created.OrderId));

            // 3) Siparişi tamamlama isteği
            var idemKeyComplete = Guid.NewGuid().ToString();
            var completeMsg = new HttpRequestMessage(
                HttpMethod.Post, $"/api/orders/{created.OrderId}/complete");
            completeMsg.Headers.Add("Idempotency-Key", idemKeyComplete);

            var completeResp = await _client.SendAsync(completeMsg);
            Assert.Equal(HttpStatusCode.OK, completeResp.StatusCode);

            var envelope = await completeResp.Content.ReadFromJsonAsync<CompleteResponseEnvelope>();
            Assert.NotNull(envelope);
            Assert.True(envelope.Success);
            Assert.Equal(created.OrderId, envelope.OrderId);
        }
    }

}
