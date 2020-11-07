let snap2gate = {};
let gate2snap = {};

// Associate a snap point with a gate element
export function associateGate(snapID, gateID) {
  snap2gate[snapID] = gateID;
  gate2snap[gateID] = snapID;
}

export function initGrids() {
  // clear element associations
  snap2gate = {};
  gate2snap = {};

  // make gates draggable
  $(".gate").draggable({
    containment: "#grid",
    scroll: false,
    snap: ".grid-snap",
    snapMode: "inner",
    snapTolerance: 10,
    revert: "invalid"
  });

  // configure droppable snap points
  document.querySelectorAll('.grid-snap').forEach(
    snap => {
      $(snap).droppable({
        // only accept gates if no gate is already on this snap point
        accept: () => {
          return !snap2gate[snap.id];
        },
        // update associations on gate dropped
        drop: (event, {draggable}) => {
          const gateID = draggable[0].id;

          // clear previous snap-gate association
          const prevSnap = gate2snap[gateID];
          if (prevSnap) {
            snap2gate[prevSnap] = null;
          }

          // associate the gate with this snap
          associateGate(snap.id, gateID);
        },
        tolerance: "fit"
      });
    }
  );

}
