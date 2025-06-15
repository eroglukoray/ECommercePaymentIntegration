
using ECommerce.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ECommerce.Test.Domain
{

    public class OrderTests
    {
        [Fact]
        public void TotalPrice_ShouldSumAllProductPrices()
        {
            // Arrange
            var order = new Order();
            order.Products.Add(new Product { Price = 10m });
            order.Products.Add(new Product { Price = 15.5m });

            // Act
            var total = order.TotalPrice;

            // Assert
            total.Should().Be(25.5m);
        }

        [Fact]
        public void NewOrder_ShouldHavePendingStatus()
        {
            var order = new Order();
            order.Status.Should().Be("Pending");
        }
    }

}
