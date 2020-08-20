using System.Collections.Generic;
using Common;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace Explorer.Utilities.ComponentTree
{
    /// <inheritdoc />
    /// <summary>
    /// A node in the panel tree.
    /// Represents a horizontal or vertical container with resizable panels.
    /// </summary>
    internal class PanelTree : IPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PanelTree" /> class.
        /// </summary>
        /// <param name="alignment">Specifies how the internal panels are going to be oriented.</param>
        internal PanelTree(Alignment alignment) => Direction = alignment;

        /// <summary>
        /// Alignment of the components in the container.
        /// Either <see cref="Horizontal" /> or <see cref="Vertical" />.
        /// </summary>
        internal enum Alignment
        {
            Horizontal,
            Vertical,
        }

        /// <summary>
        /// Gets alignment of the child panels in the container.
        /// </summary>
        [JsonProperty]
        internal Alignment Direction { get; }

        /// <summary>
        /// Gets a list of the child panels.
        /// </summary>
        [JsonProperty]
        internal List<IPanel> Children { get; } = new List<IPanel>();

        [JsonProperty]
        internal string ElementId { get; } = UniqueId.CreateUniqueId();

        /// <inheritdoc/>
        void IPanel.AcceptRenderer(PanelRenderer render) => render.RenderPanelTree(this);

        internal void AddPanel(IPanel panel) => Children.Add(panel);

        internal Panel<T> AddPanel<T>()
            where T : IComponent
        {
            var panel = new Panel<T>();
            Children.Add(panel);
            return panel;
        }
    }
}
