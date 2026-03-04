using Anthropic;
using Anthropic.Models.Messages;
using Staticsoft.Content.Abstractions;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Staticsoft.Content.Claude;

public class ClaudeTextContent<Response>(
    AnthropicClient claude,
    ClaudeContentOptions<Response> options
) : TextContent<Response>
{
    readonly AnthropicClient Claude = claude;
    readonly ClaudeContentOptions<Response> Options = options;
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
        var message = await Claude.Messages.Create(new MessageCreateParams
        {
            Model = Options.Model,
            MaxTokens = 4096,
            System = Options.SystemMessage,
            Messages = [new MessageParam { Role = Role.User, Content = userMessage }]
        });

        foreach (var block in message.Content)
            if (block.TryPickText(out var textBlock))
                return textBlock.Text;

        throw EmptyResponse();
    }

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
