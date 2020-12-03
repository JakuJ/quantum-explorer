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
        accept: (_) => {
          return !snap.classList.contains('locked');
        },
        // update associations on gate dropped
        drop: async (event, {draggable}) => {
          const gateID = draggable[0].id;

          if (snap.classList.contains('locked')) {
            console.log('Dropped on a locked snap');
          }

          if (snap.classList.contains('half')) {
            console.log('Dropped between columns, expanding');
          } else {
            console.log('Dropped on a column, reassociating');
          }

          // draggable[0].style.left = '0';
        },
        tolerance: 'fit'
      });
    }
  );

}
