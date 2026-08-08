using Amazon;
using Amazon.BedrockRuntime;
using Microsoft.Extensions.DependencyInjection;
using Staticsoft.Content.Bedrock;

namespace Staticsoft.Content.Tests;

public class BedrockStreamableContentTests : StreamableContentTests
{
    protected override IServiceCollection Services => base.Services
        .UseBedrock(
            _ => BedrockServices.Client(),
            _ => new() { ModelId = BedrockServices.ModelId() }
        );
}

public class BedrockTextContentTests : TextContentTests
{
    protected override IServiceCollection Services => base.Services
        .AddSingleton(_ => BedrockServices.Client())
        .UseBedrockModel<Reply, BedrockReplyOptions>();
}

public class BedrockReplyOptions : BedrockContentOptions<TextContentTests.Reply>
{
    public BedrockReplyOptions()
    {
        ModelId = BedrockServices.ModelId();
        SystemMessage = "You respond with JSON only, no code fences.";
    }
}

public static class BedrockServices
{
    public static AmazonBedrockRuntimeClient Client()
        => new(GetAccessKeyId(), GetSecretAccessKey(), GetRegion());

    public static string ModelId()
        => EnvVariable("ContentBedrockModelId");

    static string GetAccessKeyId()
        => EnvVariable("ContentAccessKeyId");

    static string GetSecretAccessKey()
        => EnvVariable("ContentSecretAccessKey");

    static RegionEndpoint GetRegion()
        => RegionEndpoint.GetBySystemName(EnvVariable("ContentRegion"));

    static string EnvVariable(string name)
        => Environment.GetEnvironmentVariable(name)
        ?? throw new ArgumentNullException($"Environment variable {name} is not set");
}
