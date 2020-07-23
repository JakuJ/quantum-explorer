using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace Explorer.Utilities.ComponentTree
{
    /// <inheritdoc />
    /// <summary>
    /// A leaf in the panel tree. Wraps around an <see cref="IComponent" />.
    /// </summary>
    internal class Panel : IPanel
    {
        internal Panel(IComponent component) => Component = component;

        /// <summary>
        /// Gets the component that this <see cref="Panel" /> instance wraps around.
        /// </summary>
        internal IComponent Component { get; }

        [JsonProperty]
        internal string ElementId { get; } = UniqueId.CreateUniqueId();

        /// <inheritdoc />
        void IPanel.AcceptRenderer(PanelRenderer renderer) => renderer.RenderPanel(this);
    }
}
