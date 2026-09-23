using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Assistant;
using OrderFlow.Application.Assistant.Dtos;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.Dtos;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("orders")]
[Produces("application/json")]
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    /// <summary>Cria um novo pedido.</summary>
    /// <remarks>O pedido é criado com status <c>Pendente</c> e um evento <c>order.created</c> é publicado no RabbitMQ.</remarks>
    [HttpPost]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await orderService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    /// <summary>Lista todos os pedidos.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OrderResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> List(CancellationToken cancellationToken) =>
        Ok(await orderService.ListAsync(cancellationToken));

    /// <summary>Responde perguntas em linguagem natural sobre os pedidos.</summary>
    /// <remarks>Ex.: "Quantos pedidos estão pendentes?" ou "Qual o valor total de pedidos finalizados este mês?"</remarks>
    [HttpPost("ask")]
    [ProducesResponseType<AskQuestionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<AskQuestionResponse>> Ask(
        AskQuestionRequest request,
        [FromServices] IOrderAssistant assistant,
        CancellationToken cancellationToken) =>
        Ok(await assistant.AskAsync(request, cancellationToken));

    /// <summary>Obtém os detalhes de um pedido.</summary>
    /// <remarks>Inclui o histórico de mudanças de status, em ordem cronológica.</remarks>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<OrderDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailsResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await orderService.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }
}
