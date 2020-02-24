var parametrosModal = {
    itemId: 0,
    parent: '',
}


$('#confirm-delete').on('show.bs.modal', function (e) {
    var data = $(e.relatedTarget).data();
    $('.description', this).text(data.itemDescription);

    parametrosModal.itemId = data.itemId;
    parametrosModal.parent = data.parent;

});

var deleteModal = document.querySelector("#deleteModal");
deleteModal.addEventListener("click", function (event) {
    var id = parametrosModal.itemId;

    $.post('/' + parametrosModal.parent + '/Delete/' + id).then(function () {
        var url = window.location.href.split('?');
        window.location.href = url[0] + "?registroApagado=true";
    });
});
