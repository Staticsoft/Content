using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Staticsoft.Content.Abstractions;
using Message = Staticsoft.Content.Abstractions.Message;

namespace Staticsoft.Content.Bedrock;

public class BedrockStreamableContent(
    AmazonBedrockRuntimeClient bedrock,
    BedrockContentOptions options
) : StreamableContent
{
    readonly AmazonBedrockRuntimeClient Bedrock = bedrock;
    readonly BedrockContentOptions Options = options;

    public async IAsyncEnumerable<string> Produce(IEnumerable<Message> messages)
    {
        var response = await Bedrock.ConverseStreamAsync(CreateRequest(messages));
        await foreach (var streamEvent in response.Stream)
        {
            if (streamEvent is not ContentBlockDeltaEvent deltaEvent) continue;
            if (deltaEvent.Delta?.Text is not { Length: > 0 } text) continue;
            yield return text;
        }
    }

    ConverseStreamRequest CreateRequest(IEnumerable<Message> messages)
    {
        var messagesList = messages.ToList();
        return new()
        {
            ModelId = Options.ModelId,
            System = BedrockRequest.System(messagesList),
            Messages = BedrockRequest.Messages(messagesList),
            InferenceConfig = BedrockRequest.InferenceConfig()
        };
    }
}
