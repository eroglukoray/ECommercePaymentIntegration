using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Queries
{
    public record GetProductsQuery() : IRequest<List<ProductDto>>;
}
