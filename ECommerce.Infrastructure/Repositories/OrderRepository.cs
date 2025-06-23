using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _ctx;
        public OrderRepository(ApplicationDbContext ctx) => _ctx = ctx;

        public async Task AddAsync(Order order)
        {
            _ctx.Orders.Add(order);
            await _ctx.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _ctx.Orders.Update(order);
            await _ctx.SaveChangesAsync();
        }

        public async Task<Order?> GetByIdAsync(string orderId)
        {
            return await _ctx.Orders
                             .Include(o => o.Products)
                             .FirstOrDefaultAsync(o => o.Id == orderId);
        }
    }
}
