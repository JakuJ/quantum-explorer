using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Explorer.Utilities.Composer
{
    /// <summary>
    /// Handles the associations between the grid gates and snaps.
    /// </summary>
    public class GridSnapAssoc
    {
        private readonly ILogger logger;

        private readonly Dictionary<string, string> snap2Gate = new();

        private readonly Dictionary<string, string> gate2Snap = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="GridSnapAssoc"/> class.
        /// </summary>
        /// <param name="log">A logger object.</param>
        /// <param name="gatePosChanged">Action to be called on changing the gate position.</param>
        public GridSnapAssoc(ILogger log, Action<string, string> gatePosChanged)
        {
            logger = log;
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
        /// <returns>Error code.</returns>
        public bool Associate(string snapId, string gateId)
        {
            if (snap2Gate.ContainsKey(snapId))
            {
                logger.LogError($"Snap {snapId} already associated");
                return true;
            }

            if (gate2Snap.ContainsKey(gateId))
            {
                logger.LogError($"Gate {gateId} already associated");
                return true;
            }

            snap2Gate.Add(snapId, gateId);
            gate2Snap.Add(gateId, snapId);
            logger.LogInformation($"Associated snap {snapId} with gate {gateId}");
            return false;
        }

        /// <summary>Get the GateID basing on the SnapID.</summary>
        /// <param name="snapId">Snap ID.</param>
        /// <returns>Gate ID.</returns>
        [JSInvokable]
        public string? GateId(string snapId) => snap2Gate.ContainsKey(snapId) ? snap2Gate[snapId] : null;

        /// <summary>Get the SnapID basing on the GateID.</summary>
        /// <param name="gateId">Gate ID.</param>
        /// <returns>Snap ID.</returns>
        [JSInvokable]
        public string? SnapId(string gateId) => gate2Snap.ContainsKey(gateId) ? gate2Snap[gateId] : null;

        /// <summary>Reassociate the gate with the new snap.</summary>
        /// <param name="gateId">Reassociated Gate ID.</param>
        /// <param name="snapId">The new Snap ID.</param>
        [JSInvokable]
        public void Reassociate(string gateId, string snapId)
        {
            logger.LogInformation($"Re-associating gate {gateId} to snap {snapId}");

            // Disconnect the gate ID.
            string oldSnapId = Disassociate(gateId);

            GatePositionChanged(oldSnapId, snapId);
        }

        /// <summary>
        /// Clear the dictionaries.
        /// </summary>
        public void Clear()
        {
            snap2Gate.Clear();
            gate2Snap.Clear();
        }

        /// <summary>
        /// Disconnect the gate ID from the old snap ID.
        /// </summary>
        /// <param name="gateId">The gate ID.</param>
        /// <returns>Old snap ID.</returns>
        [JSInvokable]
        public string Disassociate(string gateId)
        {
            if (gate2Snap.TryGetValue(gateId, out var oldSnapId))
            {
                snap2Gate.Remove(oldSnapId);
                gate2Snap.Remove(gateId);
                logger.LogInformation($"Disassociated gate {gateId} from snap {oldSnapId}");
                return oldSnapId;
            }

            logger.LogError("Old snap key not found while disassociating! gateId: {0}", gateId);
            throw new KeyNotFoundException(nameof(gateId));
        }
    }
}
