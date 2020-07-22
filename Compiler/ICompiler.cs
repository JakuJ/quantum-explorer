using System.Collections.Generic;
using System.Threading.Tasks;

namespace Compiler
{
    public interface ICompiler
    {
        Task Compile(string code);

        string GetCode();
        List<string> GetDiagnostics();
    }
}
