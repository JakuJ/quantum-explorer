using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Explorer.Utilities.ComponentTree
{
    public class PanelComponent : IPanel
    {
        public IComponent Component { get; }

        public PanelComponent(IComponent component)
        {
            Component = component;
        }

        public string ElementId { get; } = UniqueId.GetUniqueId();

        public List<IPanel>? Children { get; } = null;

        public string? Direction { get; } = null;

        public void AcceptRenderer(PanelRenderer renderer)
        {
            renderer.VisitResizable(this);
        }
    }
}
