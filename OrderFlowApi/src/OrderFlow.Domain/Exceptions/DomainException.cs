namespace OrderFlow.Domain.Exceptions;

/// <summary>
/// Violação de regra de negócio. Mapeada para HTTP 400 na API.
/// </summary>
public sealed class DomainException(string message) : Exception(message);
