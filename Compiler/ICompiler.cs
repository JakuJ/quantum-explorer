using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompilerExtensions
{
    public interface ICompiler
    {
        Task Compile(string filepath);

        string GetCode();
        List<string> GetDiagnostics();
    }
}
