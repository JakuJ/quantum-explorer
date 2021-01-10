using System;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// A class notifying cells and the composer about the cell menu state changes.
    /// </summary>
    public class CellMenusNotifier
    {
        /// <summary>
        /// Triggered by a cell (when a gate is added/deleted) or the composer (when the button is clicked).
        /// </summary>
        public void NotifyMenuClosed() => NotifyClosed?.Invoke();

        /// <summary>
        /// Triggered by a cell (on click).
        /// </summary>
        public void NotifyMenuOpened() => NotifyOpened?.Invoke();

        /// <summary>
        /// Notifies cells to close a menu and the composer to hide the button.
        /// </summary>
        public event Action NotifyClosed = null!;

        /// <summary>
        /// Notifies the composer that a menu is opened.
        /// Triggers to display the button behind the menu to block any other functionalities.
        /// </summary>
        public event Action NotifyOpened = null!;
    }
}
