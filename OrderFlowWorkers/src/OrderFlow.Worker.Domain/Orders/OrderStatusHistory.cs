namespace OrderFlow.Worker.Domain.Orders;

public sealed class OrderStatusHistory(Guid id, Guid orderId, OrderStatus status, DateTimeOffset dataAlteracao)
{
    public Guid Id { get; private set; } = id;
    public Guid OrderId { get; private set; } = orderId;
    public OrderStatus Status { get; private set; } = status;
    public DateTimeOffset DataAlteracao { get; private set; } = dataAlteracao;

    internal static OrderStatusHistory Create(Guid orderId, OrderStatus status, DateTimeOffset dataAlteracao) =>
        new(Guid.CreateVersion7(dataAlteracao), orderId, status, dataAlteracao);
}
