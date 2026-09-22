using OrderFlow.Domain.Exceptions;

namespace OrderFlow.Domain.Orders;

public sealed class Order
{
    public const int ClienteMaxLength = 200;
    public const int ProdutoMaxLength = 200;

    public Guid Id { get; private set; }
    public string Cliente { get; private set; } = null!;
    public string Produto { get; private set; } = null!;
    public decimal Valor { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTimeOffset DataCriacao { get; private set; }

    // Usado pelo EF Core
    private Order() { }

    public static Order Create(string cliente, string produto, decimal valor, DateTimeOffset dataCriacao)
    {
        if (string.IsNullOrWhiteSpace(cliente))
            throw new DomainException("O cliente é obrigatório.");

        if (cliente.Trim().Length > ClienteMaxLength)
            throw new DomainException($"O cliente deve ter no máximo {ClienteMaxLength} caracteres.");

        if (string.IsNullOrWhiteSpace(produto))
            throw new DomainException("O produto é obrigatório.");

        if (produto.Trim().Length > ProdutoMaxLength)
            throw new DomainException($"O produto deve ter no máximo {ProdutoMaxLength} caracteres.");

        if (valor <= 0)
            throw new DomainException("O valor deve ser maior que zero.");

        return new Order
        {
            Id = Guid.CreateVersion7(dataCriacao),
            Cliente = cliente.Trim(),
            Produto = produto.Trim(),
            Valor = valor,
            Status = OrderStatus.Pendente,
            DataCriacao = dataCriacao
        };
    }
}
