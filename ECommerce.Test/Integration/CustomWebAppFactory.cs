using ECommerce.API;              // ← Bu satır çok önemli!
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;
using System.Linq;


namespace ECommerce.Test.Integration
{
    public class CustomWebAppFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Mevcut IBalanceManagementService kaydını kaldır
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IBalanceManagementService));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Mock servisi ekle
                var mock = new Mock<IBalanceManagementService>();

                // GetProductsAsync => tek test ürünü
                mock.Setup(x => x.GetProductsAsync())
                    .ReturnsAsync(new List<ProductDto>
                    {
                        new ProductDto
                        {
                            Id = "prod-001",
                            Name = "TestProduct",
                            Description = "Desc",
                            Price = 9.99m,
                            Currency = "USD",
                            Category = "Test",
                            Stock = 100
                        }
                    });

                // PreorderAsync => orderId ve amount alır
                mock.Setup(x => x.PreorderAsync(It.IsAny<string>(), It.IsAny<int>()))
                    .ReturnsAsync("res-test");

                // CompleteOrderAsync => her zaman true
                mock.Setup(x => x.CompleteOrderAsync(It.IsAny<string>()))
                    .ReturnsAsync(true);

                services.AddSingleton(mock.Object);
            });
        }
    }
}

