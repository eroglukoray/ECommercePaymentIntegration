using AutoMapper;
using ECommerce.Application.Commands;
using ECommerce.Application.DTOs;
using ECommerce.Application.Handlers;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Mapping;
using ECommerce.Application.Queries;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ECommerce.Test.Application
{
    public class GetProductsQueryHandlerTests
    {
        [Fact]
        public async Task GetProductsQueryHandler_ReturnsListFromService()
        {
            // Arrange
            var productsFromSvc = new List<ProductDto>
            {
                new() { Id = "p1", Name = "Ürün1", Price = 10m, Currency = "USD" },
                new() { Id = "p2", Name = "Ürün2", Price = 20m, Currency = "USD" }
            };
            var svcMock = new Mock<IBalanceManagementService>();
            svcMock
                .Setup(s => s.GetProductsAsync())
                .ReturnsAsync(productsFromSvc);

            var mapperMock = new Mock<IMapper>();
            mapperMock
                .Setup(m => m.Map<List<ProductDto>>(productsFromSvc))
                .Returns(productsFromSvc);

            var handler = new GetProductsQueryHandler(
                svcMock.Object,
                mapperMock.Object
            );

            // Act
            var result = await handler.Handle(new GetProductsQuery(), CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("p1", result[0].Id);
            svcMock.Verify(s => s.GetProductsAsync(), Times.Once);
            mapperMock.Verify(m => m.Map<List<ProductDto>>(productsFromSvc), Times.Once);
        }
    }

    public class CreateOrderCommandHandlerTests
    {
        private readonly Mock<IBalanceManagementService> _balanceSvcMock;
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IIdempotencyRepository> _idemRepoMock;
        private readonly IMapper _mapper;
        private readonly CreateOrderCommandHandler _handler;

        public CreateOrderCommandHandlerTests()
        {
            // 1) BalanceService stub
            _balanceSvcMock = new Mock<IBalanceManagementService>();
            _balanceSvcMock
                .Setup(s => s.GetProductsAsync())
                .ReturnsAsync(new List<ProductDto>
                {
                    new() { Id = "prod-001", Name = "TestÜrün", Price = 5m, Currency = "USD" }
                });
            _balanceSvcMock
                .Setup(s => s.PreorderAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync("reservation-123");

            // 2) OrderRepository stub
            _orderRepoMock = new Mock<IOrderRepository>();
            _orderRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Order>()))
                .Returns(Task.CompletedTask);
            _orderRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
                .Returns(Task.CompletedTask);

            // 3) IdempotencyRepository stub
            _idemRepoMock = new Mock<IIdempotencyRepository>();

            // 4) AutoMapper real config
            var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
            _mapper = cfg.CreateMapper();

            // 5) Handler
            _handler = new CreateOrderCommandHandler(
                _balanceSvcMock.Object,
                _orderRepoMock.Object,
                _idemRepoMock.Object,
                _mapper
            );
        }

        //    [Fact]
        //    public async Task CreateOrderCommandHandler_SuccessfulFlow_ReturnsReservedStatus_And_SavesIdempotency()
        //    {
        //        // Arrange
        //        var key = Guid.NewGuid().ToString();
        //        // Before first call, no record exists
        //        _idemRepoMock
        //            .Setup(r => r.GetByKeyAsync(key))
        //            .ReturnsAsync((IdempotencyRecord)null);

        //        var cmd = new CreateOrderCommand(
        //            new CreateOrderRequest { ProductIds = new List<string> { "prod-001" } },
        //            key);

        //        // Act
        //        var result = await _handler.Handle(cmd, CancellationToken.None);

        //        // Assert
        //        Assert.Equal("Reserved", result.Status);
        //        Assert.False(string.IsNullOrWhiteSpace(result.OrderId));

        //        _orderRepoMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
        //        _orderRepoMock.Verify(r => r.UpdateAsync(
        //            It.Is<Order>(o => o.ReservationId == "reservation-123" && o.Status == "Reserved")),
        //            Times.Once);

        //        // IdempotencyRepository.SaveAsync çağıldı mı?
        //        _idemRepoMock.Verify(r => r.SaveAsync(
        //            It.Is<IdempotencyRecord>(rec =>
        //                rec.Key == key
        //                && rec.ResultJson.Contains(result.OrderId))),
        //            Times.Once);
        //    }


        //}

        public class CompleteOrderCommandHandlerTests
        {
            [Fact]
            public async Task CompleteOrderCommandHandler_SuccessfulFlow_UpdatesStatusAndReturnsEnvelope()
            {
                // Arrange
                var orderId = "order-123";
                var idemKey = Guid.NewGuid().ToString();

                // OrderRepository stub
                var orderRepoMock = new Mock<IOrderRepository>();
                var orderEntity = new Order
                {
                    Id = orderId,
                    Status = "Reserved",
                    ReservationId = "res-123"
                };
                orderRepoMock
                    .Setup(r => r.GetByIdAsync(orderId))
                    .ReturnsAsync(orderEntity);
                orderRepoMock
                    .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
                    .Returns(Task.CompletedTask);

                // BalanceService stub
                var balanceMock = new Mock<IBalanceManagementService>();
                balanceMock
                    .Setup(s => s.CompleteOrderAsync(orderId))
                    .ReturnsAsync(true);

                var handler = new CompleteOrderCommandHandler(
                    balanceMock.Object,
                    orderRepoMock.Object
                );

                var cmd = new CompleteOrderCommand(orderId, idemKey);

                // Act
                var envelope = await handler.Handle(cmd, CancellationToken.None);

                // Assert
                Assert.True(envelope.Success);
                Assert.Equal(orderId, envelope.OrderId);
                orderRepoMock.Verify(r => r.UpdateAsync(
                    It.Is<Order>(o => o.Status == "Completed")),
                    Times.Once);
            }

            [Fact]
            public async Task CompleteOrderCommandHandler_FailedFlow_ThrowsApplicationException()
            {
                // Arrange
                var orderId = "order-404";
                var idemKey = Guid.NewGuid().ToString();

                var orderRepoMock = new Mock<IOrderRepository>();
                orderRepoMock
                    .Setup(r => r.GetByIdAsync(orderId))
                    .ReturnsAsync(new Order { Id = orderId, Status = "Reserved" });

                var balanceMock = new Mock<IBalanceManagementService>();
                balanceMock
                    .Setup(s => s.CompleteOrderAsync(orderId))
                    .ReturnsAsync(false);

                var handler = new CompleteOrderCommandHandler(
                    balanceMock.Object,
                    orderRepoMock.Object
                );

                var cmd = new CompleteOrderCommand(orderId, idemKey);

                // Act & Assert
                await Assert.ThrowsAsync<ApplicationException>(async () =>
                    await handler.Handle(cmd, CancellationToken.None));
            }
        }
    }
}