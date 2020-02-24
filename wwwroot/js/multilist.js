function addItem(left, right) {
    $("#" + left + " option:selected").appendTo("#" + right + "");
    $("#" + right + " option").attr("selected", false);
    ordena(right);
}
function addallItems(left, right) {
    $("#" + left + " option").appendTo("#" + right + "");
    $("#" + right + " option").attr("selected", false);
    ordena("" + right + "");
}
function removeItem(left, right) {
    $("#" + right + " option:selected").appendTo("#" + left + "");
    $("#" + left + " option").attr("selected", false);
    ordena(left);
}
function removeallItems(left, right) {
    $("#" + right + " option").appendTo("#" + left + "");
    $("#" + left + " option").attr("selected", false);
    ordena(left);
}

function ordena(lista) {
    $("#" + lista).html($("#" + lista + " option").sort(function (a, b) {
        return a.text == b.text ? 0 : a.text < b.text ? -1 : 1
    }))
}

function seleciona(right) {
    var lista = document.getElementById(right);
    for (i = 0; i < lista.length; i++) {
        lista.options[i].selected = true;
    }

}

function seleciona2(first, second) {
    var lista1 = document.getElementById(first);
    for (i = 0; i < lista1.length; i++) {
        lista1.options[i].selected = true;
    }

    var lista2 = document.getElementById(second);
    for (i = 0; i < lista2.length; i++) {
        lista2.options[i].selected = true;
    }
}

