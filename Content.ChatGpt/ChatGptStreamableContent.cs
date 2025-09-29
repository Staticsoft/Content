using Betalgo.Ranul.OpenAI.Interfaces;
using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using Staticsoft.Content.Abstractions;

namespace Staticsoft.Content.ChatGpt;

public class ChatGptStreamableContent(
    IOpenAIService chatGpt,
    ChatGptContentOptions options
) : StreamableContent
{
    readonly IOpenAIService ChatGpt = chatGpt;
    readonly ChatGptContentOptions Options = options;

    public async IAsyncEnumerable<string> Produce(IEnumerable<Message> messages)
    {
        var request = CreateChatCompletionRequest(messages);
        var completionResult = ChatGpt.ChatCompletion.CreateCompletionAsStream(request);

        await foreach (var completion in completionResult)
        {
            if (completion.Error != null)
                throw new ContentException(completion.Error.Message);

            var choice = completion.Choices?.FirstOrDefault();
            var content = choice?.Delta?.Content;

            if (string.IsNullOrEmpty(content))
                continue;

            yield return content;
        }
    }

    ChatCompletionCreateRequest CreateChatCompletionRequest(IEnumerable<Message> messages)
        => new()
        {
            Messages = ConvertMessages(messages).ToList(),
            Model = Options.Model,
            Stream = true
        };

    static IEnumerable<ChatMessage> ConvertMessages(IEnumerable<Message> messages)
        => messages.Select(ConvertMessage);

    static ChatMessage ConvertMessage(Message message)
        => message.Author switch
        {
            Message.Type.System => ChatMessage.FromSystem(message.Text),
            Message.Type.User => ChatMessage.FromUser(message.Text),
            Message.Type.Assistant => ChatMessage.FromAssistant(message.Text),
            _ => throw new ContentException($"Unknown message author type: {message.Author}")
        };
}
