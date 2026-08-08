using Microsoft.Extensions.DependencyInjection;
using Staticsoft.Content.Memory;

namespace Staticsoft.Content.Tests;

public class MemoryStreamableContentTests : StreamableContentTests
{
    protected override IServiceCollection Services => base.Services
        .UseMemory();
}

public class MemoryTextContentTests : TextContentTests
{
    protected override IServiceCollection Services => base.Services
        .UseMemoryModel(_ => new Reply { Answer = "pong" });
}
