using ECommerce.Application.DTOs;
using MediatR;


namespace ECommerce.Application.Commands
{
    /// <summary>
    /// Ödeme tamamlamayı tetikleyen komut.
    /// </summary>
    public record CompleteOrderCommand(
        string OrderId,
        string IdempotencyKey
    ) : IRequest<CompleteResponseEnvelope>;
}
