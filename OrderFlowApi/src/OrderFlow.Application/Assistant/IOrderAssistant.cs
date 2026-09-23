using OrderFlow.Application.Assistant.Dtos;

namespace OrderFlow.Application.Assistant;

public interface IOrderAssistant
{
    Task<AskQuestionResponse> AskAsync(AskQuestionRequest request, CancellationToken cancellationToken);
}
