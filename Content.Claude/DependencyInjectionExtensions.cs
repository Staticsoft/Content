using Anthropic;
using Microsoft.Extensions.DependencyInjection;
using Staticsoft.Content.Abstractions;

namespace Staticsoft.Content.Claude;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection UseClaude(this IServiceCollection services, ClaudeContentOptions options)
        => services
            .AddSingleton(options)
            .AddSingleton(new AnthropicClient { ApiKey = options.ApiKey })
            .AddSingleton<StreamableContent, ClaudeStreamableContent>();

    public static IServiceCollection UseClaudeModel<Response, Options>(this IServiceCollection services)
        where Options : ClaudeContentOptions<Response>
        => services
            .AddSingleton<TextContent<Response>, ClaudeTextContent<Response>>()
            .AddSingleton<ClaudeContentOptions<Response>, Options>();
}
