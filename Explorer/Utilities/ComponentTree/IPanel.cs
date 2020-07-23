namespace Explorer.Utilities.ComponentTree
{
    /// <summary>
    /// Allows an object to be processed by a <see cref="PanelRenderer" />.
    /// </summary>
    internal interface IPanel
    {
        /// <summary>
        /// Render an entity using a <see cref="PanelRenderer" />.
        /// </summary>
        /// <param name="render">A <see cref="PanelRenderer" /> instance used to render this entity.</param>
        void AcceptRenderer(PanelRenderer render);
    }
}
