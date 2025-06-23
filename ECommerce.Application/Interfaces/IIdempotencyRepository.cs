
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Repositories
{
    public interface IIdempotencyRepository
    {
        Task<IdempotencyRecord?> GetByKeyAsync(string key);
        Task SaveAsync(IdempotencyRecord record);
    }
}
