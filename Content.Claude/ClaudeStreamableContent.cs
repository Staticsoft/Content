using Anthropic;
using Anthropic.Models.Messages;
using Staticsoft.Content.Abstractions;
using Message = Staticsoft.Content.Abstractions.Message;

namespace Staticsoft.Content.Claude;

public class ClaudeStreamableContent(
    AnthropicClient claude,
    ClaudeContentOptions options
) : StreamableContent
{
    readonly AnthropicClient Claude = claude;
    readonly ClaudeContentOptions Options = options;

    public async IAsyncEnumerable<string> Produce(IEnumerable<Message> messages)
    {
        var request = CreateRequest(messages);
        await foreach (var streamEvent in Claude.Messages.CreateStreaming(request))
        {
            if (!streamEvent.TryPickContentBlockDelta(out var delta)) continue;
            if (!delta.Delta.TryPickText(out var textDelta)) continue;
            yield return textDelta.Text;
        }
    }

    MessageCreateParams CreateRequest(IEnumerable<Message> messages)
    {
        var messagesList = messages.ToList();
        var systemText = string.Join("\n", messagesList
            .Where(m => m.Author == Message.Type.System)
            .Select(m => m.Text));

        return new MessageCreateParams
        {
            Model = Options.Model,
            MaxTokens = 4096,
            System = systemText,
            Messages = messagesList
                .Where(m => m.Author != Message.Type.System)
                .Select(ConvertMessage)
                .ToList(),
            Thinking = Options.Reasoning != ClaudeContentOptions.ReasoningLevel.None
                ? new ThinkingConfigParam(new ThinkingConfigEnabled(ToBudgetTokens(Options.Reasoning)))
                : null
        };
    }

    static int ToBudgetTokens(ClaudeContentOptions.ReasoningLevel reasoning)
        => reasoning switch
        {
            ClaudeContentOptions.ReasoningLevel.Low => 1024,
            ClaudeContentOptions.ReasoningLevel.Medium => 8000,
            ClaudeContentOptions.ReasoningLevel.High => 32000,
            _ => throw new NotSupportedException($"{nameof(ClaudeContentOptions.ReasoningLevel)} '{reasoning}' is not supported")
        };

    static MessageParam ConvertMessage(Message message)
        => message.Author switch
        {
            Message.Type.User => new MessageParam { Role = Role.User, Content = message.Text },
            Message.Type.Assistant => new MessageParam { Role = Role.Assistant, Content = message.Text },
            _ => throw new ContentException($"Unknown message author type: {message.Author}")
        };
}
