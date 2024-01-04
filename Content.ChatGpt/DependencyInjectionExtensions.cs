using Microsoft.Extensions.DependencyInjection;
using OpenAI.Extensions;
using Staticsoft.Content.Abstractions;

namespace Staticsoft.Content.ChatGpt;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection UseChatGpt(this IServiceCollection services, string apiKey) => services
            .AddSingleton<TextContent, ChatGptTextContent>()
            .AddOpenAIService(settings => { settings.ApiKey = apiKey; }).Services;
}
