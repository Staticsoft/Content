using OpenAI.ObjectModels;

namespace Staticsoft.Content.ChatGpt;

public class ChatGptTextContentOptions<Response>
{
    public string Model { get; init; } = Models.Gpt_4;
    public string SystemMessage { get; init; } = string.Empty;
}
