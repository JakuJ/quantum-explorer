using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Explorer.Utilities.ComponentTree
{
    /// <inheritdoc />
    /// <summary>
    /// A node in the panel tree.
    /// Represents a horizontal or vertical container with resizable panels.
    /// </summary>
    public class PanelTree : IPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PanelTree"/> class.
        /// </summary>
        /// <param name="alignment">Specifies how the internal panels are going to be oriented.</param>
        public PanelTree(Alignment alignment)
        {
            Direction = alignment;
            Children = new List<IPanel>();
        }

        /// <summary>
        /// Alignment of the components in the container.
        /// Either <see cref="Horizontal"/> or <see cref="Vertical"/>.
        /// </summary>
        public enum Alignment
        {
            /// <summary>
            /// Signifies that internal panels will be aligned horizontally.
            /// </summary>
            Horizontal,

            /// <summary>
            /// Signifies that internal panels will be aligned vertically.
            /// </summary>
            Vertical,
        }

        /// <inheritdoc/>
        public string ElementId { get; } = UniqueId.CreateUniqueId();

        /// <summary>
        /// Gets alignment of the child panels in the container.
        /// </summary>
        public Alignment Direction { get; }

        /// <summary>
        /// Gets a list of the child panels.
        /// </summary>
        public List<IPanel> Children { get; }

        /// <summary>
        /// Add a panel to the end of the container.
        /// </summary>
        /// <param name="panel">The panel to be added.</param>
        public void AddPanel(IPanel panel) => Children.Add(panel);

        /// <summary>
        /// Add a component to the end of the container.
        /// The component is wrapped with the <see cref="Panel"/> class.
        /// </summary>
        /// <param name="component">The component to be added.</param>
        public void AddPanel(IComponent component) => Children.Add(new Panel(component));

        /// <inheritdoc/>
        public void AcceptRenderer(PanelRenderer render) => render.RenderPanelTree(this);
    }
}
