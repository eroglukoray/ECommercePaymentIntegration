using Microsoft.AspNetCore.Mvc;
using MediatR;
using ECommerce.Application.Commands;
using ECommerce.Application.DTOs;
using ECommerce.Application.Queries;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    public OrdersController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>
    /// Yeni bir sipariş oluşturur ve bakiyeden tutar bloke eder.
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType(typeof(OrderResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResultDto>> Create(
        [FromBody] CreateOrderRequest request,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest(new ProblemDetails
            {
                Title = "Missing Idempotency-Key header",
                Status = StatusCodes.Status400BadRequest
            });

        var cmd = new CreateOrderCommand(request, idempotencyKey);
        var result = await _mediator.Send(cmd);

        // CreatedAtAction için GetById endpoint'ine referans
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.OrderId },
            result);
    }
    /// <summary>
    /// Siparişi ID'sine göre getirir.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResultDto>> GetById(string id)
    {
        var query = new GetOrderByIdQuery(id);
        var dto = await _mediator.Send(query);
        if (dto is null)
            return NotFound(new ProblemDetails
            {
                Title = $"Order with ID '{id}' not found",
                Status = StatusCodes.Status404NotFound
            });
        return Ok(dto);
    }

    /// <summary>
    /// Var olan siparişi tamamlar ve bakiyeyi nihai olarak çeker.
    /// </summary>
    [HttpPost("{orderId}/complete")]
    [ProducesResponseType(typeof(CompleteResponseEnvelope), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CompleteResponseEnvelope>> Complete(
        string orderId,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest(new ProblemDetails
            {
                Title = "Missing Idempotency-Key header",
                Status = StatusCodes.Status400BadRequest
            });

        var cmd = new CompleteOrderCommand(orderId, idempotencyKey);
        var result = await _mediator.Send(cmd);

        return Ok(result);
    }
}
