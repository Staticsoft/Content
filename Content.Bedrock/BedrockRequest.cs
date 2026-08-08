using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Staticsoft.Content.Abstractions;
using Message = Staticsoft.Content.Abstractions.Message;

namespace Staticsoft.Content.Bedrock;

static class BedrockRequest
{
    const int MaxTokens = 4096;

    public static List<SystemContentBlock> System(IEnumerable<Message> messages)
        => messages
            .Where(message => message.Author == Message.Type.System)
            .Select(message => new SystemContentBlock { Text = message.Text })
            .ToList();

    public static List<Amazon.BedrockRuntime.Model.Message> Messages(IEnumerable<Message> messages)
        => messages
            .Where(message => message.Author != Message.Type.System)
            .Select(ConvertMessage)
            .ToList();

    public static InferenceConfiguration InferenceConfig()
        => new() { MaxTokens = MaxTokens };

    static Amazon.BedrockRuntime.Model.Message ConvertMessage(Message message)
        => new()
        {
            Role = ConvertRole(message.Author),
            Content = [new ContentBlock { Text = message.Text }]
        };

    static ConversationRole ConvertRole(Message.Type author)
        => author switch
        {
            Message.Type.User => ConversationRole.User,
            Message.Type.Assistant => ConversationRole.Assistant,
            _ => throw new ContentException($"Unknown message author type: {author}")
        };
}
