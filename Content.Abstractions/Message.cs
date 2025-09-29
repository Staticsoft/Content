namespace Staticsoft.Content.Abstractions;

public class Message
{
    public required Type Author { get; init; }
    public required string Text { get; init; }

    public enum Type
    {
        System,
        User,
        Assistant
    }
}