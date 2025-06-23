namespace ECommerce.Domain.Entities
{
    public class IdempotencyRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Key { get; set; } = default!;
        public string ResultJson { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
