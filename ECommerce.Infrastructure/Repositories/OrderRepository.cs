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

        public async Task<Order?> GetByIdAsync(string id)
            => await _ctx.Orders.Include(o => o.Products)
                .FirstOrDefaultAsync(o => o.Id == id);

        public async Task UpdateAsync(Order order)
        {
            // 1) Veritabanından zaten var olan Order’ı yükle (ilişkili ürünleri de dahil et)
            var existing = await _ctx.Orders
                .Include(o => o.Products)
                .FirstOrDefaultAsync(o => o.Id == order.Id)
                ?? throw new KeyNotFoundException($"Order {order.Id} bulunamadı.");

            // 2) Sadece Status ve ReservationId güncelle (ürünleri yeniden ekleme!)
            existing.Status = order.Status;
            existing.ReservationId = order.ReservationId;

            // 3) SaveChanges — EF sadece bu iki alanı güncelleyecek
            await _ctx.SaveChangesAsync();
        }
    }
}
