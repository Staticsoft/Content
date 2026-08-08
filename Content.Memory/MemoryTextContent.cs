using Staticsoft.Content.Abstractions;

namespace Staticsoft.Content.Memory;

/// <summary>
/// In-process TextContent: produces the response with a function
/// configured at registration, standing in for a model that returns
/// JSON matching the <typeparamref name="Response"/> type.
/// </summary>
public class MemoryTextContent<Response>(
    Func<string, Response> produce
) : TextContent<Response>
{
    readonly Func<string, Response> ProduceResponse = produce;

    public Task<Response> Produce(string requirements)
        => Task.FromResult(ProduceResponse(requirements));
}
