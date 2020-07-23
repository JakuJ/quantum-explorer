using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Explorer.Utilities.ComponentTree
{
    public class PanelTree : IPanel
    {
        public string ElementId { get; } = UniqueId.GetUniqueId();

        public Orientation ChildOrientation { get; }

        public string? Direction => ChildOrientation switch
        {
            Orientation.Vertical => "vertical",
            _                    => "horizontal",
        };

        public List<IPanel> Children { get; }

        public enum Orientation
        {
            Horizontal,
            Vertical,
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PanelTree"/> class.
        /// </summary>
        /// <param name="orientation">Specifies how the internal panels are going to be oriented.</param>
        public PanelTree(Orientation orientation)
        {
            ChildOrientation = orientation;
            Children = new List<IPanel>();
        }

        public void AddPanel(IPanel panel) => Children.Add(panel);

        public void AddPanel(IComponent component) => Children.Add(new PanelComponent(component));

        public void AcceptRenderer(PanelRenderer render) => render.VisitPanelTree(this);
    }
}
