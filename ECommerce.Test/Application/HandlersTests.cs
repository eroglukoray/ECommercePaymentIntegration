using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

using ECommerce.Application.Commands;
using ECommerce.Application.DTOs;
using ECommerce.Application.Handlers;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;

namespace ECommerce.Test.Application
{
    public class HandlersTests
    {
        [Fact]
        public async Task GetProductsQueryHandler_ReturnsListFromService()
        {
            // Arrange
            var expected = new List<ProductDto>
            {
                new ProductDto {
                    Id          = "1",
                    Name        = "A",
                    Description = "D",
                    Price       = 1m,
                    Currency    = "USD",
                    Category    = "C",
                    Stock       = 1
                }
            };
            var mockService = new Mock<IBalanceManagementService>();
            mockService
                .Setup(s => s.GetProductsAsync())
                .ReturnsAsync(expected);

            var handler = new GetProductsQueryHandler(mockService.Object);

            // Act
            var result = await handler.Handle(
                new ECommerce.Application.Queries.GetProductsQuery(),
                CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task CreateOrderCommandHandler_SuccessfulFlow_ReturnsReservedStatus()
        {
            // Arrange
            var dto = new ProductDto
            {
                Id = "1",
                Name = "A",
                Description = "D",
                Price = 10m,
                Currency = "USD",
                Category = "C",
                Stock = 1
            };
            var mockService = new Mock<IBalanceManagementService>();
            mockService
                .Setup(s => s.GetProductsAsync())
                .ReturnsAsync(new List<ProductDto> { dto });
            mockService
                .Setup(s => s.PreorderAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync("res-1");
       
            var mockRepo = new Mock<IOrderRepository>();

            mockRepo
                .Setup(r => r.AddAsync(It.IsAny<Order>()))
                .Returns(Task.CompletedTask);
            mockRepo
                .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
                .Returns(Task.CompletedTask);

            var handler = new CreateOrderCommandHandler(
                mockService.Object,
                mockRepo.Object
            );

            var command = new CreateOrderCommand(
                new CreateOrderRequest { ProductIds = new List<string> { "1" } }
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Status.Should().Be("Reserved");
            result.OrderId.Should().NotBeNullOrEmpty();

            mockRepo.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
            mockRepo.Verify(r => r.UpdateAsync(
                It.Is<Order>(o =>
                    o.ReservationId == "res-1" &&
                    o.Status == "Reserved")),
                Times.Once);
        }

        [Fact]
        public async Task CompleteOrderCommandHandler_SuccessfulFlow_ReturnsTrueAndUpdatesStatus()
        {
            // Arrange
            var order = new Order
            {
                Id = "1",
                Products = new List<Product>(),
                Status = "Reserved",
                ReservationId = "res-1"
            };
            var mockService = new Mock<IBalanceManagementService>();
            mockService
                .Setup(s => s.CompleteOrderAsync(order.ReservationId))
                .ReturnsAsync(true);

            var mockRepo = new Mock<IOrderRepository>();
            mockRepo
                .Setup(r => r.GetByIdAsync(order.Id))
                .ReturnsAsync(order);
            mockRepo
                .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
                .Returns(Task.CompletedTask);

            var handler = new CompleteOrderCommandHandler(
                mockService.Object,
                mockRepo.Object
            );
            var command = new CompleteOrderCommand(order.Id);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            order.Status.Should().Be("Completed");
            mockRepo.Verify(r => r.UpdateAsync(order), Times.Once);
        }

        [Fact]
        public async Task CompleteOrderCommandHandler_OrderNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var mockService = new Mock<IBalanceManagementService>();
            var mockRepo = new Mock<IOrderRepository>();
            mockRepo
                .Setup(r => r.GetByIdAsync("notfound"))
                .ReturnsAsync((Order)null);

            var handler = new CompleteOrderCommandHandler(
                mockService.Object,
                mockRepo.Object
            );
            var command = new CompleteOrderCommand("notfound");

            // Act & Assert
            await handler
                .Invoking(h => h.Handle(command, CancellationToken.None))
                .Should()
                .ThrowAsync<KeyNotFoundException>();
        }
    }
}
