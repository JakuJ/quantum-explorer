using System;
using System.Collections.Generic;
using Microsoft.JSInterop;

namespace Explorer.Utilities.Composer
{
    /// <summary>
    /// Handles the associations between the grid gates and snaps.
    /// </summary>
    public class GridSnapAssoc
    {
        private readonly Dictionary<string, string> snap2Gate = new Dictionary<string, string>();

        private readonly Dictionary<string, string> gate2Snap = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GridSnapAssoc"/> class.
        /// </summary>
        /// <param name="gatePosChanged">Action to be called on changing the gate position.</param>
        public GridSnapAssoc(Action<string, string> gatePosChanged)
        {
            GatePositionChanged = gatePosChanged;
        }

        /// <summary>
        /// Gets the action which is called when the gate position gets changed.
        /// </summary>
        private Action<string, string> GatePositionChanged { get; }

        /// <summary>
        /// Associate the snap with the gate.
        /// As this method is called, they can not be associated yet.
        /// </summary>
        /// <param name="snapId">Snap ID.</param>
        /// <param name="gateId">Gate ID.</param>
        public void Associate(string snapId, string gateId)
        {
            snap2Gate.Add(snapId, gateId);
            gate2Snap.Add(gateId, snapId);
        }

        /// <summary>Get the GateID basing on the SnapID.</summary>
        /// <param name="snapId">Snap ID.</param>
        /// <returns>Gate ID.</returns>
        [JSInvokable]
        public string? GateId(string snapId)
        {
            return snap2Gate.ContainsKey(snapId) ? snap2Gate[snapId] : null;
        }

        /// <summary>Get the SnapID basing on the GateID.</summary>
        /// <param name="gateId">Gate ID.</param>
        /// <returns>Snap ID.</returns>
        [JSInvokable]
        public string? SnapId(string gateId)
        {
            return gate2Snap.ContainsKey(gateId) ? gate2Snap[gateId] : null;
        }

        /// <summary>Reassociate the gate with the new snap.</summary>
        /// <param name="gateId">Reassociated Gate ID.</param>
        /// <param name="snapId">The new Snap ID.</param>
        [JSInvokable]
        public void Reassociate(string gateId, string snapId)
        {
            // Disconnect the gate ID from the old snap ID.
            var oldSnapId = gate2Snap[gateId];
            snap2Gate.Remove(oldSnapId);
            gate2Snap.Remove(gateId);

            // Connect the gate ID to the new snap ID.
            Associate(snapId, gateId);
            GatePositionChanged.Invoke(oldSnapId, snapId);
        }

        /// <summary>
        /// Clear the dictionaries.
        /// </summary>
        public void Clear()
        {
            snap2Gate.Clear();
            gate2Snap.Clear();
        }
    }
}
