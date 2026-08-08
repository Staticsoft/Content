using Staticsoft.Content.Abstractions;
using Staticsoft.Testing;
using Xunit;

namespace Staticsoft.Content.Tests;

public abstract class StreamableContentTests : TestBase<StreamableContent>
{
    const string ParrotInstruction = "You are a parrot. Repeat the user's message exactly, with no quotes and no extra words.";
    const string LongMessage = "The quick brown fox jumps over the lazy dog near the river bank.";

    [Fact]
    public async Task ProducesNonEmptyResponse()
    {
        var response = await Produce(ParrotInstruction, "pong");
        Assert.NotEqual(string.Empty, response);
    }

    [Fact]
    public async Task FollowsConversationInstructions()
    {
        var response = await Produce(ParrotInstruction, "pong");
        Assert.Contains("pong", response, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task StreamsResponseInMultipleChunks()
    {
        var chunks = 0;
        await foreach (var _ in SUT.Produce(Conversation(ParrotInstruction, LongMessage))) chunks += 1;
        Assert.True(chunks > 1, $"Expected the response in multiple chunks, got {chunks}");
    }

    [Fact]
    public async Task ProducesResponseForConsecutiveRequests()
    {
        var first = await Produce(ParrotInstruction, "first");
        var second = await Produce(ParrotInstruction, "second");
        Assert.Contains("first", first, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("second", second, StringComparison.OrdinalIgnoreCase);
    }

    async Task<string> Produce(string system, string user)
    {
        var response = string.Empty;
        await foreach (var chunk in SUT.Produce(Conversation(system, user)))
        {
            response += chunk;
        }
        return response;
    }

    static Message[] Conversation(string system, string user)
        =>
        [
            new() { Author = Message.Type.System, Text = system },
            new() { Author = Message.Type.User, Text = user }
        ];
}
