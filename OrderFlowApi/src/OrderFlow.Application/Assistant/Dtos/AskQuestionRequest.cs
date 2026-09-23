using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Application.Assistant.Dtos;

public sealed class AskQuestionRequest
{
    [Required(ErrorMessage = "A pergunta é obrigatória.")]
    [MaxLength(500, ErrorMessage = "A pergunta deve ter no máximo 500 caracteres.")]
    public string Pergunta { get; init; } = string.Empty;
}
