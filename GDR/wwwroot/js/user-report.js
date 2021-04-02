$("#ReportList").select2({
    placeholder: "Selecione um relatório",
    minimumInputLength: 0,
    allowClear: true,
    language: "pt-BR",
    width: '100%'
});


$("#DatabaseList").select2({
    placeholder: "Selecione um ou mais bancos de dados ou deixe em branco para todos",
    minimumInputLength: 0,
    allowClear: true,
    language: "pt-BR",
    width: '100%'
});

/*
var reportId = $('#ReportList').val();
if (reportId != null) {
    GetDatabases(reportId)
}
*/



$('#ReportList').on("select2:select", function (e) {
    GetDatabases($(this).val());
});

function GetDatabases(reportId) {

    var xhr = new XMLHttpRequest();
    xhr.open("POST", "/UserReports/GetDatabases");
    xhr.setRequestHeader("Content-type", "application/x-www-form-urlencoded"); 
    xhr.send('reportId=' + reportId);
    xhr.addEventListener("load", function () {
        if (xhr.status == 200) {
            var databaseList = xhr.responseText;
            var selectItens = "";
            var databaseList = JSON.parse(databaseList);
            databaseList.forEach(function (database) {
                selectItens += "<option value='" + database.value + "'>" + database.text + "</option>";
            });
            var selectBox = 'DatabaseList';
            $("#" + selectBox).html(selectItens);
            
        }
        else {
            console.log(xhr.status);
            console.log(xhr.responseText);
        }
    });

}



function setDivMessageStatus(status, message) {

    orderBy = document.querySelectorAll("#orderBy");

    divMessage = document.querySelector("#divMessage");

    if (status) {

        orderBy.forEach(function (orderBy) {
            orderBy.remove("invisible");
        });

        
        divMessage.classList.remove("invisible");
        divMessage.innerHTML = message;
    }
    else {

        orderBy.forEach(function (orderBy) {
            orderBy.classList.remove("invisible");
        });
        
        divMessage.classList.add("invisible");
        divMessage.innerHTML = "";
    }
}


function ChangeLineColorRemove(line) {
    line.classList.remove("table-change-line-color");
}

function ChangeLineColorAdd(line) {
    line.classList.add("table-change-line-color");
}
