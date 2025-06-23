using AutoMapper;
using ECommerce.Application.Commands;
using ECommerce.Application.DTOs;
using ECommerce.Application.Handlers;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Mapping;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ECommerce.Test.Application
{
    public class CreateOrderCommandHandler_IdempotencyTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IIdempotencyRepository _idemRepo;
        private readonly Mock<IBalanceManagementService> _balanceSvcMock;
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly IMapper _mapper;
        private readonly CreateOrderCommandHandler _handler;

        public CreateOrderCommandHandler_IdempotencyTests()
        {
            // 1) InMemory DbContext
            var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ApplicationDbContext(opts);

            // 2) Gerçek IdempotencyRepository
            _idemRepo = new IdempotencyRepository(_dbContext);

            // 3) BalanceService stub
            _balanceSvcMock = new Mock<IBalanceManagementService>();
            _balanceSvcMock
                .Setup(s => s.GetProductsAsync())
                .ReturnsAsync(new List<ProductDto> {
                    new() { Id="prod-001", Name="Test", Price=1m, Currency="USD" }
                });
            _balanceSvcMock
                .Setup(s => s.PreorderAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync("res-1");

            // 4) OrderRepository stub
            _orderRepoMock = new Mock<IOrderRepository>();
            _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<Order>()))
                          .Returns(Task.CompletedTask);
            _orderRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Order>()))
                          .Returns(Task.CompletedTask);

            // 5) AutoMapper
            var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
            _mapper = cfg.CreateMapper();

            // 6) Handler
            _handler = new CreateOrderCommandHandler(
                _balanceSvcMock.Object,
                _orderRepoMock.Object,
                _idemRepo,
                _mapper
            );
        }

        [Fact]
        public async Task Handler_Is_Idempotent_When_CalledTwiceWithSameKey()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var cmd = new CreateOrderCommand(
                new CreateOrderRequest { ProductIds = new List<string> { "prod-001" } },
                key
            );

            // Act
            var firstResult = await _handler.Handle(cmd, CancellationToken.None);
            var secondResult = await _handler.Handle(cmd, CancellationToken.None);

            // Assert: aynı OrderId döndürülmeli
            Assert.Equal(firstResult.OrderId, secondResult.OrderId);

            // Sadece bir idempotency kaydı olmalı
            var records = _dbContext.IdempotencyRecords.ToList();
            Assert.Single(records);
            Assert.Equal(key, records[0].Key);

            // Kayıtta serialize edilmiş JSON, OrderId ile eşleşiyor mu?
            var dto = System.Text.Json.JsonSerializer
                .Deserialize<OrderResultDto>(records[0].ResultJson);
            Assert.NotNull(dto);
            Assert.Equal(firstResult.OrderId, dto!.OrderId);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}
