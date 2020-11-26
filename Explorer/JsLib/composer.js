// initialize the .NET objects references
let gridRef = null;
let assocRef = null;

export function setReferences(_gridRef, _assocRef) {
  gridRef = _gridRef;
  assocRef = _assocRef;
}

export function initGrids() {
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
        accept: async () => {
          return !await assocRef.invokeMethodAsync('GateId', snap.id);
        },
        // update associations on gate dropped
        drop: async (event, {draggable}) => {
          const gateID = draggable[0].id;

          if(snap.classList.contains('half'))
          {
            console.log('dropped on half snap');

            // insert a new column
            await gridRef.invokeMethodAsync('Expand', snap.id);
          }
          await assocRef.invokeMethodAsync('Reassociate', gateID, snap.id);
        },
        tolerance: "fit"
      });
    }
  );

}
