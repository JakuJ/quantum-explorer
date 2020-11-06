let snap2gate = {};
let gate2snap = {};

export function associateGate(snapID, gateID) {
  snap2gate[snapID] = gateID;
  gate2snap[gateID] = snapID;
}

export function initGrid() {
  $(".gate").draggable({
    containment: "#grid",
    scroll: false,
    snap: ".grid-snap",
    snapMode: "inner",
    snapTolerance: 10,
    revert: "invalid"
  })

  document.querySelectorAll('.grid-snap').forEach(
    snap => {
      $(snap).droppable({
        accept: gate => {
          return !snap2gate[snap.id];
        },
        drop: (event, {draggable}) => {
          const gateID = draggable[0].id;

          // clear previous snap-gate association
          let prevSnap = gate2snap[gateID];
          if (prevSnap) {
            snap2gate[prevSnap] = null;
          }

          // associate the gate with this snap
          associateGate(snap.id, gateID);
        },
        tolerance: "fit"
      });
    }
  )

}
