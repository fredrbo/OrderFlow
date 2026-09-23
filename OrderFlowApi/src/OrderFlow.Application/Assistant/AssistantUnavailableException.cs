namespace OrderFlow.Application.Assistant;

public sealed class AssistantUnavailableException(Exception innerException)
    : Exception("O assistente de pedidos está indisponível no momento.", innerException);
