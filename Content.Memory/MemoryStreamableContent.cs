using Staticsoft.Content.Abstractions;

namespace Staticsoft.Content.Memory;

/// <summary>
/// Deterministic in-process model: replies by repeating the last
/// non-system message, streamed in small chunks. It behaves like a real
/// model that follows a "repeat the user's message" instruction, which is
/// what the shared test suite demands of every implementation.
/// </summary>
public class MemoryStreamableContent : StreamableContent
{
    const int ChunkSize = 8;

    public async IAsyncEnumerable<string> Produce(IEnumerable<Message> messages)
    {
        var text = LastConversationMessage(messages).Text;
        for (var start = 0; start < text.Length; start += ChunkSize)
        {
            yield return text.Substring(start, Math.Min(ChunkSize, text.Length - start));
        }
        await Task.CompletedTask;
    }

    static Message LastConversationMessage(IEnumerable<Message> messages)
        => messages.LastOrDefault(message => message.Author != Message.Type.System)
        ?? throw new ContentException("No user or assistant messages to reply to");
}
