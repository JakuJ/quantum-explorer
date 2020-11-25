using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;

namespace Compiler
{
    /// <summary>
    /// A class for keeping information about quantum operation.
    /// </summary>
    public class OperationState
    {
        /// <summary>
        /// Gets a name of the operation.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the parent operation.
        /// </summary>
        public OperationState Parent { get; set; } = null;

        /// <summary>
        /// Gets a list of operations that are run inside this operation.
        /// </summary>
        public List<OperationState> Children { get; } = new List<OperationState>();

        /// <summary>
        /// Gets or sets a list of complex numbers that represent quantum states of arguments represented by index.
        /// </summary>
        public List<(int Idx, Complex Value)> Arguments { get; set; }

        /// <summary>
        /// Gets or sets a list of complex numbers that represent quantum states of results represented by index.
        /// </summary>
        public List<(int Idx, Complex Value)> Results { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationState"/> class.
        /// </summary>
        /// <param name="name">Name of the operation</param>
        public OperationState(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Adds child operation.
        /// </summary>
        /// <param name="child">Child operation run in this operation.</param>
        [ExcludeFromCodeCoverage]
        public void AddOperation(OperationState child)
        {
            child.Parent = this;
            Children.Add(child);
        }
    }
}
