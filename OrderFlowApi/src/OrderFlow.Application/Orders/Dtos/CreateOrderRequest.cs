namespace OrderFlow.Application.Orders.Dtos;

public sealed record CreateOrderRequest(string Cliente, string Produto, decimal Valor);
