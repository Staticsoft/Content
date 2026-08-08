using Microsoft.Extensions.DependencyInjection;
using Staticsoft.Content.Abstractions;

namespace Staticsoft.Content.Memory;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection UseMemory(this IServiceCollection services)
        => services
            .AddSingleton<StreamableContent, MemoryStreamableContent>();

    public static IServiceCollection UseMemoryModel<Response>(
        this IServiceCollection services,
        Func<string, Response> produce
    ) => services
        .AddSingleton<TextContent<Response>>(new MemoryTextContent<Response>(produce));
}
