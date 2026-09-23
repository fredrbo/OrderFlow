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

    public void StartProcessing()
    {
        if (Status != OrderStatus.Pendente)
            throw new DomainException($"Apenas pedidos pendentes podem iniciar o processamento. Status atual: {Status}.");

        Status = OrderStatus.Processando;
    }

    public void Finish(DateTimeOffset finishedAt)
    {
        if (Status != OrderStatus.Processando)
            throw new DomainException($"Apenas pedidos em processamento podem ser finalizados. Status atual: {Status}.");

        Status = OrderStatus.Finalizado;
        DataFinalizacao = finishedAt;
    }
}
