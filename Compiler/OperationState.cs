using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;

namespace Compiler
{
    /// <summary>
    /// A class for keeping information about quantum operation.
    /// </summary>
    public class OperationState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationState"/> class.
        /// </summary>
        /// <param name="name">Name of the operation.</param>
        public OperationState(string name) => Name = name;

        /// <summary>
        /// Gets a name of the operation.
        /// </summary>
        [JsonProperty("N")]
        public string Name { get; }

        /// <summary>
        /// Gets a list of operations that are run inside this operation.
        /// </summary>
        [JsonProperty("C")]
        public List<OperationState> Children { get; } = new();

        /// <summary>
        /// Gets or sets a list of complex numbers that represent quantum states of arguments represented by index.
        /// </summary>
        [JsonProperty("A")]
        public List<(int Idx, Complex Value)>? Arguments { get; set; }

        /// <summary>
        /// Gets or sets a list of complex numbers that represent quantum states of results represented by index.
        /// </summary>
        [JsonProperty("R")]
        public List<(int Idx, Complex Value)>? Results { get; set; }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is OperationState other)
            {
                return Name == other.Name
                    && Children.SequenceEqual(other.Children)
                    && (Arguments == null || other.Arguments == null || Arguments.SequenceEqual(other.Arguments))
                    && (Results == null || other.Results == null || Results.SequenceEqual(other.Results));
            }

            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }
}
