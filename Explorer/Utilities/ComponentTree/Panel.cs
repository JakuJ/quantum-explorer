using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace Explorer.Utilities.ComponentTree
{
    /// <inheritdoc />
    /// <summary>
    /// A leaf in the panel tree. Wraps around an <see cref="IComponent"/>.
    /// </summary>
    public class Panel : IPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Panel"/> class.
        /// </summary>
        /// <param name="component">The component to be wrapped.</param>
        public Panel(IComponent component) => Component = component;

        /// <summary>
        /// Gets the component that this instance wraps around.
        /// </summary>
        [JsonIgnore]
        public IComponent Component { get; }

        /// <inheritdoc/>
        public string ElementId { get; } = UniqueId.CreateUniqueId();

        /// <inheritdoc/>
        public void AcceptRenderer(PanelRenderer renderer) => renderer.RenderPanel(this);
    }
}
