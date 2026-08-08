using Staticsoft.Content.Abstractions;
using Staticsoft.Testing;
using Xunit;

namespace Staticsoft.Content.Tests;

public abstract class TextContentTests : TestBase<TextContent<TextContentTests.Reply>>
{
    public class Reply
    {
        public string Answer { get; init; } = string.Empty;
    }

    protected const string Requirements =
        """
        Return a JSON object with a single string field "answer"
        containing exactly the word "pong".
        """;

    [Fact]
    public async Task ProducesTypedResponse()
    {
        var reply = await SUT.Produce(Requirements);
        Assert.Equal("pong", reply.Answer, ignoreCase: true);
    }
}
