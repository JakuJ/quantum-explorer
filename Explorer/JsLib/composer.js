export function initGrid() {
  $(".gate").draggable({
    containment: "#grid",
    scroll: false,
    snap: ".grid-snap",
    snapMode: "inner",
    snapTolerance: 25
  })
}
