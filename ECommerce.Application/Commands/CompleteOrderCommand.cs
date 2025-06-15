using MediatR;


namespace ECommerce.Application.Commands
{
    public record CompleteOrderCommand(string OrderId) : IRequest<bool>;
}
