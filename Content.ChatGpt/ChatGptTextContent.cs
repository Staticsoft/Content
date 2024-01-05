using OpenAI.Interfaces;
using OpenAI.ObjectModels.RequestModels;
using Staticsoft.Content.Abstractions;

namespace Staticsoft.Content.ChatGpt;

public class ChatGptTextContent(
    IOpenAIService chatGpt,
    ChatGptTextContentOptions options
) : TextContent
{
    readonly IOpenAIService ChatGpt = chatGpt;
    readonly ChatGptTextContentOptions Options = options;

    public async Task<string> Produce(string requirements)
    {
        var completionResult = await ChatGpt.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages = new List<ChatMessage>
            {
                ChatMessage.FromUser(requirements)
            },
            Model = Options.Model
        });

        if (completionResult.Error != null) throw new ContentException(completionResult.Error.Message);

        return Normalize(completionResult.Choices.First().Message.Content);
    }

    static string Normalize(string text) => text
        .Replace(Environment.NewLine, "\n")
        .Replace("\n", Environment.NewLine);
}
