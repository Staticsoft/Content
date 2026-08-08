namespace Staticsoft.Content.Bedrock;

public class BedrockContentOptions<Response>
{
    public string ModelId { get; init; } = string.Empty;
    public string SystemMessage { get; init; } = string.Empty;
}

public class BedrockContentOptions
{
    public required string ModelId { get; init; }
}
