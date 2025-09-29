using Betalgo.Ranul.OpenAI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Staticsoft.Content.Abstractions;

namespace Staticsoft.Content.ChatGpt;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection UseChatGpt(this IServiceCollection services, ChatGptContentOptions options)
        => services
            .AddSingleton(options)
            .AddOpenAIService(settings => { settings.ApiKey = options.ApiKey; }).Services
            .AddSingleton<StreamableContent, ChatGptStreamableContent>();

    public static IServiceCollection UseChatGptModel<Response, Options>(this IServiceCollection services)
        where Options : ChatGptContentOptions<Response>
        => services
            .AddSingleton<TextContent<Response>, ChatGptTextContent<Response>>()
            .AddSingleton<ChatGptContentOptions<Response>, Options>();
}
