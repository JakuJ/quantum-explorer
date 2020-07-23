using System.Collections.Generic;

namespace Explorer.Utilities.ComponentTree
{
    public interface IPanel
    {
        string ElementId { get; }
        List<IPanel>? Children { get; }
        string? Direction { get; }
        void AcceptRenderer(PanelRenderer render);
    }
}
