using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class IdempotencyRepository : IIdempotencyRepository
    {
        private readonly ApplicationDbContext _ctx;
        public IdempotencyRepository(ApplicationDbContext ctx) => _ctx = ctx;

        public async Task<IdempotencyRecord?> GetByKeyAsync(string key) =>
            await _ctx.IdempotencyRecords
                      .AsNoTracking()
                      .FirstOrDefaultAsync(r => r.Key == key);

        public async Task<IEnumerable<IdempotencyRecord>> GetAllAsync() =>
            await _ctx.IdempotencyRecords
                      .AsNoTracking()
                      .OrderBy(r => r.CreatedAt)
                      .ToListAsync();

        public async Task SaveAsync(IdempotencyRecord record)
        {
            _ctx.IdempotencyRecords.Add(record);
            await _ctx.SaveChangesAsync();
        }
    }
}
