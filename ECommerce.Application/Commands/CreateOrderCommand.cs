using ECommerce.Application.DTOs;
using MediatR;


namespace ECommerce.Application.Commands
{
    public record CreateOrderCommand(
        CreateOrderRequest Request,
        string IdempotencyKey
    ) : IRequest<OrderResultDto>;
}
