using System.Runtime.Loader;

namespace Compiler
{
    /// <inheritdoc cref="AssemblyLoadContext"/>
    public class QSharpLoadContext : AssemblyLoadContext
    {
        /// <inheritdoc cref="AssemblyLoadContext" />
        /// <summary>Initializes a new instance of the <see cref="QSharpLoadContext"/> class.</summary>
        public QSharpLoadContext() : base(true) { }
    }
}
