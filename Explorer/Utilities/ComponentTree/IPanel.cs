namespace Explorer.Utilities.ComponentTree
{
    /// <summary>
    /// Allows an object to be processed by a <see cref="PanelRenderer"/>.
    /// </summary>
    public interface IPanel
    {
        /// <summary>
        /// Gets the ID to be used for the surrounding HTML tag.
        /// </summary>
        string ElementId { get; }

        /// <summary>
        /// Render an entity using a <see cref="PanelRenderer"/>.
        /// </summary>
        /// <param name="render">A <see cref="PanelRenderer"/> used to render this entity.</param>
        void AcceptRenderer(PanelRenderer render);
    }
}
