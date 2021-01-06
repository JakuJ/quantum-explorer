let gridRef = null;

export function setReference(ref) {
  gridRef = ref;
}

export function initializeToggles() {
  $('#expand-toggle').bootstrapToggle({
    on: 'Intrinsics only',
    off: 'Show custom',
  });
}

export function initGrids() {
  // make gates draggable
  $('.gate').draggable({
    containment: '#grid',
    scroll: false,
    snap: '.grid-snap',
    snapMode: 'inner',
    snapTolerance: 10,
    revert: 'invalid'
  });

  // configure droppable snap points
  document.querySelectorAll('.grid-snap').forEach(
    snap => {
      $(snap).droppable({
        accept: () => {
          return !snap.classList.contains('locked');
        },
        // update associations on gate dropped
        drop: async (event, {draggable}) => {
          const gateID = draggable[0].id;

          if (snap.classList.contains('half')) {
            await gridRef.invokeMethodAsync('Expand', gateID, snap.id);
          } else {
            await gridRef.invokeMethodAsync('Move', gateID, snap.id);
          }
        },
        tolerance: 'fit'
      });
    }
  );

}
