using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Staticsoft.Content.Abstractions;
using System.Text.Encodings.Web;
using System.Text.Json;
using Message = Staticsoft.Content.Abstractions.Message;

namespace Staticsoft.Content.Bedrock;

public class BedrockTextContent<Response>(
    AmazonBedrockRuntimeClient bedrock,
    BedrockContentOptions<Response> options
) : TextContent<Response>
{
    readonly AmazonBedrockRuntimeClient Bedrock = bedrock;
    readonly BedrockContentOptions<Response> Options = options;
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
        var messages = new List<Message>
        {
            new() { Author = Message.Type.System, Text = Options.SystemMessage },
            new() { Author = Message.Type.User, Text = userMessage }
        };
        var response = await Bedrock.ConverseAsync(new ConverseRequest
        {
            ModelId = Options.ModelId,
            System = BedrockRequest.System(messages),
            Messages = BedrockRequest.Messages(messages),
            InferenceConfig = BedrockRequest.InferenceConfig()
        });

        return response.Output?.Message?.Content?.FirstOrDefault(block => block.Text is { Length: > 0 })?.Text
            ?? throw EmptyResponse();
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
