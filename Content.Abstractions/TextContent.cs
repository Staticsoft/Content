using System.Threading.Tasks;

namespace Staticsoft.Content.Abstractions;

public interface TextContent<Model>
{
    Task<Model> Produce(string requirements);
}
