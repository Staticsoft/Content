using Amazon.BedrockRuntime;
using Microsoft.Extensions.DependencyInjection;
using Staticsoft.Content.Abstractions;

namespace Staticsoft.Content.Bedrock;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection UseBedrock(
        this IServiceCollection services,
        Func<IServiceProvider, AmazonBedrockRuntimeClient> client,
        Func<IServiceProvider, BedrockContentOptions> options
    ) => services
        .AddSingleton(client)
        .AddSingleton(options)
        .AddSingleton<StreamableContent, BedrockStreamableContent>();

    public static IServiceCollection UseBedrockModel<Response, Options>(this IServiceCollection services)
        where Options : BedrockContentOptions<Response>
        => services
            .AddSingleton<TextContent<Response>, BedrockTextContent<Response>>()
            .AddSingleton<BedrockContentOptions<Response>, Options>();
}
