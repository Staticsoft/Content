namespace Staticsoft.Content.ChatGpt;

public class ChatGptContentOptions<Response>
{
    public string Model { get; init; } = string.Empty;
    public string SystemMessage { get; init; } = string.Empty;
}

public class ChatGptContentOptions
{
    public required string ApiKey { get; init; }
    public required string Model { get; init; }
}