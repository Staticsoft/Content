using Betalgo.Ranul.OpenAI.Interfaces;
using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using Betalgo.Ranul.OpenAI.ObjectModels.ResponseModels;
using Staticsoft.Content.Abstractions;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Staticsoft.Content.ChatGpt;

public class ChatGptTextContent<Response>(
    IOpenAIService chatGpt,
    ChatGptContentOptions<Response> options
) : TextContent<Response>
{
    readonly IOpenAIService ChatGpt = chatGpt;
    readonly ChatGptContentOptions<Response> Options = options;
    readonly JsonSerializerOptions DeserializationOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        AllowTrailingCommas = true
    };

    public async Task<Response> Produce(string userMessage)
    {
        var response = await GetResponse(userMessage);
        return DeserializeResponse(response);
    }

    async Task<string> GetResponse(string userMessage)
    {
        var completionResult = await CreateCompletion(userMessage);
        return completionResult.Choices.First().Message.Content ?? throw EmptyResponse();
    }

    async Task<ChatCompletionCreateResponse> CreateCompletion(string userMessage)
    {
        var request = CreateChatCompletionRequest(userMessage);
        var completionResult = await ChatGpt.ChatCompletion.CreateCompletion(request);
        if (completionResult.Error != null) throw UnableToCreateCompletion(completionResult);

        return completionResult;
    }

    ChatCompletionCreateRequest CreateChatCompletionRequest(string userMessage)
        => new()
        {
            Messages = CreateMessages(userMessage),
            Model = Options.Model,
            ChatResponseFormat = ChatCompletionCreateRequest.ResponseFormats.Json
        };

    List<ChatMessage> CreateMessages(string userMessage) => userMessage switch
    {
        "" or null => [ChatMessage.FromUser(userMessage!)],
        _ => [ChatMessage.FromSystem(Options.SystemMessage), ChatMessage.FromUser(userMessage)]
    };

    Response DeserializeResponse(string content) => Try
        .Return(() => JsonSerializer.Deserialize<Response>(FormatJsonString(content), DeserializationOptions))
        .On<JsonException>(exception => UnableToDeserialize(content, exception))
        .Result() ?? throw UnableToDeserialize(content);

    static string FormatJsonString(string input)
        => input.Length < "```json```".Length
        ? input
        : (input[..7], input[^3..]) switch
        {
            ("```json", "```") => input[7..^3],
            _ => input
        };

    static ContentException UnableToCreateCompletion(ChatCompletionCreateResponse completionResult)
        => new(completionResult.Error!.Message);

    static ContentException UnableToDeserialize(string content)
        => new(SerializationErrorMessage(content));

    static ContentException UnableToDeserialize(string content, Exception innerException)
        => new(SerializationErrorMessage(content), innerException);

    static string SerializationErrorMessage(string content)
        => $"""
            Unable to deserialize string into {typeof(Response).FullName} type.
            {content}
            """;

    static ContentException EmptyResponse()
        => new("The response was empty");
}
