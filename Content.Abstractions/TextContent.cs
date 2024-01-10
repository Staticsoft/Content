using System.Threading.Tasks;

namespace Staticsoft.Content.Abstractions;

public interface TextContent<Response>
{
    Task<Response> Produce(string requirements);
}
