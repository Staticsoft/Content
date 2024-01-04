using System.Threading.Tasks;

namespace Staticsoft.Content.Abstractions;

public interface TextContent
{
    Task<string> Produce(string requirements);
}
