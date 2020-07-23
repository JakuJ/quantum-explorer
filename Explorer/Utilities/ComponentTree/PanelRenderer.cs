using Explorer.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Explorer.Utilities.ComponentTree
{
    public class PanelRenderer
    {
        private RenderTreeBuilder builder;
        private string? currentClass;

        public RenderFragment Render(PanelTree panel) => builder =>
        {
            this.builder = builder;
            currentClass = panel.ChildOrientation == PanelTree.Orientation.Horizontal ? "split-horizontal" : "split-content";

            foreach (IPanel panel in panel.Children)
            {
                panel.AcceptRenderer(this);
            }
        };

        public void VisitResizable(Resizable resizable)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "id", resizable.ElementId);

            if (currentClass != null)
            {
                builder.AddAttribute(2, "class", $"split {currentClass}");
            }

            builder.OpenComponent(3, resizable.GetType());
            builder.CloseComponent();

            builder.CloseElement();
        }

        public void VisitPanelTree(PanelTree tree)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "id", tree.ElementId);

            if (currentClass != null)
            {
                builder.AddAttribute(2, "class", $"split {currentClass}");
            }

            currentClass = tree.ChildOrientation == PanelTree.Orientation.Horizontal ? "split-horizontal" : "split-content";
            foreach (IPanel panel in tree.Children)
            {
                panel.AcceptRenderer(this);
            }

            builder.CloseElement();
        }
    }
}
