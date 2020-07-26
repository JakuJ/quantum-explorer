using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace Explorer.Utilities.ComponentTree
{
    /// <inheritdoc />
    /// <summary>
    /// A leaf in the panel tree. Wraps around an <see cref="IComponent" />.
    /// </summary>
    internal class Panel<T> : IPanel
        where T : IComponent
    {
        /// <summary>
        /// Gets or sets the rendered instance of the component.
        /// This property is null until the panel is actually rendered using a <see cref="PanelRenderer"/>.
        /// </summary>
        internal T Component { get; set; } = default!;

        [JsonProperty]
        internal string ElementId { get; } = UniqueId.CreateUniqueId();

        /// <inheritdoc />
        void IPanel.AcceptRenderer(PanelRenderer renderer) => renderer.RenderPanel(this);
    }
}
