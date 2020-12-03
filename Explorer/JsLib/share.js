export function showSharePopOver(link) {
    $("#link-placeholder").attr("value", link);

    $('#popoverShareButton').popover({
        html: true,
        sanitize: false,
        placement: 'bottom',
        container: 'body',
        content: function () {
            return $("#popover-content").html();
        }
    });

    $('#popoverShareButton').popover('show');
}

export function initPopOverDestroyer() {
    $("html").on("mouseup", function (e) {
        if ($(e.target).parents('.popover').length == 0) {
            $(".popover").each(function () {
                $(this).popover("dispose");
            });
        }
    });
}
