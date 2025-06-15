using ECommerce.Application.DTOs;

namespace ECommerce.Infrastructure.Services
{
    public class ProductListEnvelope
    {
        public bool Success { get; set; }
        public List<ProductDto> Data { get; set; } = new();
    }
}
