using OrderFlow.Worker.Domain.Exceptions;

namespace OrderFlow.Worker.Domain.Orders;

public sealed class Order(
    Guid id,
    string cliente,
    string produto,
    decimal valor,
    OrderStatus status,
    DateTimeOffset dataCriacao,
    DateTimeOffset? dataFinalizacao = null)
{
    public Guid Id { get; private set; } = id;
    public string Cliente { get; private set; } = cliente;
    public string Produto { get; private set; } = produto;
    public decimal Valor { get; private set; } = valor;
    public OrderStatus Status { get; private set; } = status;
    public DateTimeOffset DataCriacao { get; private set; } = dataCriacao;
    public DateTimeOffset? DataFinalizacao { get; private set; } = dataFinalizacao;

    private readonly List<OrderStatusHistory> _historico = [];
    public IReadOnlyCollection<OrderStatusHistory> Historico => _historico.AsReadOnly();

    public void StartProcessing(DateTimeOffset startedAt)
    {
        if (Status != OrderStatus.Pendente)
            throw new DomainException($"Apenas pedidos pendentes podem iniciar o processamento. Status atual: {Status}.");

        ChangeStatus(OrderStatus.Processando, startedAt);
    }

    public void Finish(DateTimeOffset finishedAt)
    {
        if (Status != OrderStatus.Processando)
            throw new DomainException($"Apenas pedidos em processamento podem ser finalizados. Status atual: {Status}.");

        ChangeStatus(OrderStatus.Finalizado, finishedAt);
        DataFinalizacao = finishedAt;
    }

    private void ChangeStatus(OrderStatus status, DateTimeOffset changedAt)
    {
        Status = status;
        _historico.Add(OrderStatusHistory.Create(Id, status, changedAt));
    }
}
