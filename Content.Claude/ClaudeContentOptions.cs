namespace Staticsoft.Content.Claude;

public class ClaudeContentOptions<Response>
{
    public string Model { get; init; } = string.Empty;
    public string SystemMessage { get; init; } = string.Empty;
}

public class ClaudeContentOptions
{
    public required string ApiKey { get; init; }
    public required string Model { get; init; }
    public ReasoningLevel Reasoning { get; init; } = ReasoningLevel.None;

    public enum ReasoningLevel
    {
        None,
        Low,
        Medium,
        High
    }
}
