using Microsoft.AspNetCore.Mvc;
using MediatR;
using ECommerce.Application.Commands;
using ECommerce.Application.DTOs;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _med;
    public OrdersController(IMediator med) => _med = med;

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest req)
    {
        var res = await _med.Send(new CreateOrderCommand(req));
        return CreatedAtAction(null, res);
    }

    [HttpPost("{reservationId}/complete")]
    public async Task<IActionResult> Complete(string reservationId)
    {
        await _med.Send(new CompleteOrderCommand(reservationId));
        return NoContent();
    }
}
