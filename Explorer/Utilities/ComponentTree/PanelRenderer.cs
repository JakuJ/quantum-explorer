using System.Collections.Generic;
using Explorer.Templates;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Explorer.Utilities.ComponentTree
{
    public class PanelRenderer
    {
        private readonly Stack<string> classes = new Stack<string>();
        private int sequence = 0;

        private int Sequence => sequence++;

        private RenderTreeBuilder? builder;

        public RenderFragment Render(PanelTree panel) => treeBuilder =>
        {
            builder = treeBuilder;
            classes.Push(panel.ChildOrientation == PanelTree.Orientation.Horizontal ? "split-horizontal" : "split-content");

            foreach (IPanel child in panel.Children)
            {
                child.AcceptRenderer(this);
            }
        };

        public void VisitResizable(PanelComponent panel)
        {
            builder!.OpenElement(Sequence, "div");
            builder.AddAttribute(Sequence, "id", panel.ElementId);

            if (classes.Count > 0)
            {
                builder.AddAttribute(Sequence, "class", $"split {classes.Peek()}");
            }

            builder.OpenComponent<Resizable>(Sequence);

            void PanelComponent(RenderTreeBuilder builder2)
            {
                builder2.OpenComponent(Sequence, panel.Component.GetType());
                builder2.CloseComponent();
            }

            builder.AddAttribute(Sequence, "ChildContent", (RenderFragment)PanelComponent);

            builder.CloseComponent();
            builder.CloseElement();
        }

        public void VisitPanelTree(PanelTree tree)
        {
            builder!.OpenElement(Sequence, "div");
            builder.AddAttribute(Sequence, "id", tree.ElementId);

            if (classes.Count > 0)
            {
                builder.AddAttribute(Sequence, "class", $"split {classes.Peek()}");
            }

            classes.Push(tree.ChildOrientation == PanelTree.Orientation.Horizontal ? "split-horizontal" : "split-content");
            foreach (IPanel panel in tree.Children)
            {
                panel.AcceptRenderer(this);
            }

            classes.Pop();
            builder.CloseElement();
        }
    }
}
