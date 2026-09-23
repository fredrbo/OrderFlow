namespace OrderFlow.Domain.Orders;

public sealed class OrderStatusHistory
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTimeOffset DataAlteracao { get; private set; }

    private OrderStatusHistory() { }

    internal static OrderStatusHistory Create(Guid orderId, OrderStatus status, DateTimeOffset dataAlteracao) =>
        new()
        {
            Id = Guid.CreateVersion7(dataAlteracao),
            OrderId = orderId,
            Status = status,
            DataAlteracao = dataAlteracao
        };
}
