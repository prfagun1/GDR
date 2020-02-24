var tableFilterButton = document.querySelector(".table-filter-box");


tableFilterButton.addEventListener("input", function () {
    var filter = document.querySelectorAll(".tr-filter");
    var regularExpression = new RegExp(this.value, "i")

    if (this.value.length > 1) {
        for (var i = 0; i < filter.length; i++) {
            var tdFilter = filter[i].querySelectorAll("td");
            var found = false;

            tdFilter.forEach(function (td) {
                if (regularExpression.test(td.textContent)) {
                    found = true;
                }
            });


            if (!found) {
                filter[i].classList.add("invisible");
            }
            else {
                filter[i].classList.remove("invisible");
            }

        }
    }
    else {
        for (var i = 0; i < filter.length; i++) {
            filter[i].classList.remove("invisible");
        }
    }

});


///Order: 0 = asc     1 = desc
function sortTable(table, trId, order) {

    var table, rows, switching, i, x, y, shouldSwitch;
    table = document.getElementById(table);

    switching = true;
    /*Make a loop that will continue until
    no switching has been done:*/
    while (switching) {
        //start by saying: no switching is done:
        switching = false;
        rows = table.rows;
        /*Loop through all table rows (except the
        first, which contains table headers):*/
        for (i = 1; i < (rows.length - 1); i++) {
            //start by saying there should be no switching:
            shouldSwitch = false;
            /*Get the two elements you want to compare,
            one from current row and one from the next:*/
            x = rows[i].getElementsByTagName("TD")[trId];
            y = rows[i + 1].getElementsByTagName("TD")[trId];
            //check if the two rows should switch place:

            if (order === 0) {
                shouldSwitch = sortTableAsc(x, y);
                if (shouldSwitch) {
                    break;
                }
            }

            if (order === 1) {
                shouldSwitch = sortTableDesc(x, y);
                if (shouldSwitch) {
                    break;
                }
            }
        }

        if (shouldSwitch) {
            /*If a switch has been marked, make the switch
            and mark that a switch has been done:*/
            rows[i].parentNode.insertBefore(rows[i + 1], rows[i]);
            switching = true;
        }
    }
}


function sortTableAsc(x, y) {
    //para inteiros
    var first = parseFloat(x.innerHTML.replace('.', '').replace(',', '.'));
    var second = parseFloat(y.innerHTML.replace('.', '').replace(',', '.'));

    if (isNaN(first) || isNaN(second)) {
        //Para texto
        if (x.innerHTML.toLowerCase() > y.innerHTML.toLowerCase()) {
            //if so, mark as a switch and break the loop:
            return true;

        }
    }
    else {
        //Para numeros
        if (first > second) {
            //if so, mark as a switch and break the loop:
            return true;
        }
    }
}

function sortTableDesc(x, y) {
    var first = parseFloat(x.innerHTML.replace('.', '').replace(',', '.'));
    var second = parseFloat(y.innerHTML.replace('.', '').replace(',', '.'));

    if (isNaN(first) || isNaN(second)) {
        if (x.innerHTML.toLowerCase() < y.innerHTML.toLowerCase()) {
            return true;
        }
    }
    else {
        if (first < second) {
            return true;
        }
    }
    return false;
}