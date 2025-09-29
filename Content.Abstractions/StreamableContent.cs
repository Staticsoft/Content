using System.Collections.Generic;

namespace Staticsoft.Content.Abstractions;

public interface StreamableContent
{
    IAsyncEnumerable<string> Produce(IEnumerable<Message> messages);
}
