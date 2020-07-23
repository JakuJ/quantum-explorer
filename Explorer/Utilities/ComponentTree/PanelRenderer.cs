using System.Collections.Generic;
using Explorer.Templates;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Explorer.Utilities.ComponentTree
{
    /// <summary>
    /// A class that processes <see cref="IPanel" /> component trees and render them appropriately.
    /// </summary>
    internal class PanelRenderer
    {
        private readonly Stack<string> classes = new Stack<string>();
        private RenderTreeBuilder? builder;
        private int sequence;

        private int Sequence => sequence++;

        /// <summary>
        /// Generate content for a razor component inside a <see cref="Panel" /> wrapper.
        /// </summary>
        /// <param name="panel">A wrapper around a razor component.</param>
        public void RenderPanel(Panel panel)
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

        /// <summary>
        /// Recursively generate content for all nodes in the provided component tree.
        /// </summary>
        /// <param name="tree">A tree of component panels.</param>
        public void RenderPanelTree(PanelTree tree)
        {
            builder!.OpenElement(Sequence, "div");
            builder.AddAttribute(Sequence, "id", tree.ElementId);

            if (classes.Count > 0)
            {
                builder.AddAttribute(Sequence, "class", $"split {classes.Peek()}");
            }

            classes.Push(tree.Direction == PanelTree.Alignment.Horizontal ? "split-horizontal" : "split-content");
            foreach (IPanel panel in tree.Children)
            {
                panel.AcceptRenderer(this);
            }

            classes.Pop();
            builder.CloseElement();
        }

        internal RenderFragment Render(PanelTree panel)
        {
            return treeBuilder =>
            {
                builder = treeBuilder;
                classes.Push(panel.Direction == PanelTree.Alignment.Horizontal ? "split-horizontal" : "split-content");

                foreach (IPanel child in panel.Children)
                {
                    child.AcceptRenderer(this);
                }
            };
        }
    }
}
