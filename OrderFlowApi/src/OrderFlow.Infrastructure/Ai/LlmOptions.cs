using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Infrastructure.Ai;

public sealed class LlmOptions
{
    public const string SectionName = "Llm";

    [Required, Url] public string Endpoint { get; init; } = "http://127.0.0.1:11434";
    [Required] public string Model { get; init; } = "qwen2.5:7b";
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(3);
    [Range(1, 10)] public int MaxToolIterations { get; init; } = 5;
}
