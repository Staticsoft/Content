using Microsoft.Extensions.DependencyInjection;
using OpenAI.Extensions;
using Staticsoft.Content.Abstractions;

namespace Staticsoft.Content.ChatGpt;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection UseChatGpt(this IServiceCollection services, string apiKey) => services
        .AddOpenAIService(settings => { settings.ApiKey = apiKey; }).Services;

    public static IServiceCollection UseChatGptModel<Response, Options>(this IServiceCollection services)
        where Options : ChatGptTextContentOptions<Response>
        => services
            .AddSingleton<TextContent<Response>, ChatGptTextContent<Response>>()
            .AddSingleton<ChatGptTextContentOptions<Response>, Options>();
}
