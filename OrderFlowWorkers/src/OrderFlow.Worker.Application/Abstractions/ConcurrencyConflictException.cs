namespace OrderFlow.Worker.Application.Abstractions;

public sealed class ConcurrencyConflictException(Exception innerException)
    : Exception("O registro foi alterado por outro processo.", innerException);
